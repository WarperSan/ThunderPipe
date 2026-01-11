using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ThunderPipe.DTOs;

namespace ThunderPipe.Utils;

/// <summary>
/// Class holding the API calls to Thunderstore
/// </summary>
internal static class ThunderstoreAPI
{
	/// <summary>
	/// Validates the icon
	/// </summary>
	public static async Task<ValidateIconResponse?> ValidateIcon(
		string path,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var data = await File.ReadAllBytesAsync(path, cancellationToken);

		var payload = new ValidateIconRequest { Data = Convert.ToBase64String(data) };

		var request = builder
			.Copy()
			.Post()
			.ToEndpoint(ThunderstoreClient.API_VALIDATE_ICON)
			.WithJSON(payload)
			.Build();

		return await ThunderstoreClient.SendRequest<ValidateIconResponse>(
			request,
			cancellationToken
		);
	}

	/// <summary>
	/// Validates the manifest
	/// </summary>
	public static async Task<ValidateManifestResponse?> ValidateManifest(
		string path,
		string team,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var data = await File.ReadAllBytesAsync(path, cancellationToken);

		var payload = new ValidateManifestRequest
		{
			AuthorName = team,
			Data = Convert.ToBase64String(data),
		};

		var request = builder
			.Copy()
			.Post()
			.ToEndpoint(ThunderstoreClient.API_VALIDATE_MANIFEST)
			.WithJSON(payload)
			.Build();

		return await ThunderstoreClient.SendRequest<ValidateManifestResponse>(
			request,
			cancellationToken
		);
	}

	/// <summary>
	/// Validates the README
	/// </summary>
	public static async Task<ValidateReadmeResponse?> ValidateReadme(
		string path,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var data = await File.ReadAllBytesAsync(path, cancellationToken);

		var payload = new ValidateReadmeRequest { Data = Convert.ToBase64String(data) };

		var request = builder
			.Copy()
			.Post()
			.ToEndpoint(ThunderstoreClient.API_VALIDATE_README)
			.WithJSON(payload)
			.Build();

		return await ThunderstoreClient.SendRequest<ValidateReadmeResponse>(
			request,
			cancellationToken
		);
	}

	/// <summary>
	/// Initiates a multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CreateMultipartUpload.html">Multipart upload initiation</a> step
	/// </remarks>
	public static Task<InitialUploadResponse?> InitiateMultipartUpload(
		string path,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var fileInfo = new FileInfo(path);

		var payload = new InitialUploadRequest
		{
			File = Path.GetFileName(path),
			FileSize = fileInfo.Length,
		};

		var request = builder
			.Copy()
			.Post()
			.ToEndpoint(ThunderstoreClient.API_INITIATE_UPLOAD)
			.WithJSON(payload)
			.Build();

		return ThunderstoreClient.SendRequest<InitialUploadResponse>(request, cancellationToken);
	}

	/// <summary>
	/// Uploads every part of the file
	/// </summary>
	/// <remarks>
	/// This is simply a helper method to simplify using <see cref="ThunderstoreAPI.UploadPart"/>
	/// </remarks>
	public static async Task<UploadPartResponse[]> UploadParts(
		string file,
		InitialUploadResponse.UploadPartModel[] parts,
		CancellationToken cancellationToken
	)
	{
		var uploadTasks = new List<Task<UploadPartResponse?>>();

		await using (var stream = File.OpenRead(file))
		{
			foreach (var part in parts)
			{
				stream.Seek(part.Offset, SeekOrigin.Begin);

				var task = UploadPart(
					stream,
					part.PartNumber,
					part.Size,
					part.Url,
					cancellationToken
				);

				uploadTasks.Add(task);
			}
		}

		var uploadedParts = await Task.WhenAll(uploadTasks).WaitAsync(cancellationToken);

		return uploadedParts.OfType<UploadPartResponse>().ToArray();
	}

	/// <summary>
	/// Uploads the single part
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_UploadPart.html">Upload part</a> step
	/// </remarks>
	public static async Task<UploadPartResponse?> UploadPart(
		Stream stream,
		int id,
		int size,
		string url,
		CancellationToken cancellationToken
	)
	{
		const int BLOCK_SIZE = ushort.MaxValue;
		var chunk = new MemoryStream();

		using var reader = new BinaryReader(stream, Encoding.Default, true);
		using var md5 = MD5.Create();
		md5.Initialize();

		var remainingSize = size;

		while (remainingSize > BLOCK_SIZE)
		{
			remainingSize -= BLOCK_SIZE;

			var bytes = reader.ReadBytes(BLOCK_SIZE);

			md5.TransformBlock(bytes, 0, BLOCK_SIZE, null, 0);
			await chunk.WriteAsync(bytes, cancellationToken);
		}

		var finalBytes = reader.ReadBytes(remainingSize);
		md5.TransformFinalBlock(finalBytes, 0, remainingSize);

		if (md5.Hash == null)
			throw new NullReferenceException($"MD5 hashing failed for part #{id}.");

		var hash = md5.Hash;
		await chunk.WriteAsync(finalBytes, cancellationToken);
		chunk.Position = 0;

		var content = new StreamContent(chunk);
		content.Headers.ContentMD5 = hash;
		content.Headers.ContentLength = size;

		var request = new RequestBuilder().ToUrl(url).Put().WithContent(content).Build();

		var response = await ThunderstoreClient.SendRequest(request, cancellationToken);

		var etag = response.Headers.ETag?.Tag;

		if (etag == null)
			throw new NullReferenceException("Expected the header 'ETag' to be set.");

		return new UploadPartResponse { ETag = etag, PartNumber = id };
	}

	/// <summary>
	/// Aborts the multipart upload
	/// </summary>
	public static async Task AbortMultipartUpload(
		string uuid,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var request = builder
			.Copy()
			.Post()
			.ToEndpoint(ThunderstoreClient.API_ABORT_UPLOAD.Replace("{UUID}", uuid))
			.Build();

		await ThunderstoreClient.SendRequest(request, cancellationToken);
	}

	/// <summary>
	/// Finishes the multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CompleteMultipartUpload.html">Multipart upload completion</a> step
	/// </remarks>
	public static async Task<bool> FinishMultipartUpload(
		string uuid,
		UploadPartResponse[] parts,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var payload = new FinishUploadRequest { Parts = parts };

		var request = builder
			.Copy()
			.Post()
			.ToEndpoint(ThunderstoreClient.API_FINISH_UPLOAD.Replace("{UUID}", uuid))
			.WithJSON(payload)
			.Build();

		var response = await ThunderstoreClient.SendRequest(request, cancellationToken);

		return response.StatusCode == HttpStatusCode.OK;
	}

	/// <summary>
	/// Submits the package
	/// </summary>
	public static Task<SubmitPackageResponse?> SubmitPackage(
		string author,
		string community,
		string[] categories,
		bool hasNsfw,
		string uploadUUID,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var payload = new SubmitPackageRequest
		{
			AuthorName = author,
			Communities = [community],
			CommunityCategories = new Dictionary<string, string[]> { [community] = categories },
			HasNsfwContent = hasNsfw,
			UploadUUID = uploadUUID,
		};

		var request = builder
			.Copy()
			.Post()
			.ToEndpoint(ThunderstoreClient.API_SUBMIT_PACKAGE)
			.WithJSON(payload)
			.Build();

		return ThunderstoreClient.SendRequest<SubmitPackageResponse>(request, cancellationToken);
	}

	/// <summary>
	/// Finds the community with the slug
	/// </summary>
	public static async Task<FindCommunityResponse.PageItemModel?> FindCommunity(
		string slug,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var tempBuilder = builder.Copy().Get().ToEndpoint(ThunderstoreClient.API_COMMUNITY_PAGE);

		string? currentCursor = null;

		do
		{
			var request = tempBuilder.Copy().AddParameter("cursor", currentCursor).Build();

			var response = await ThunderstoreClient.SendRequest<FindCommunityResponse>(
				request,
				cancellationToken
			);

			if (response == null)
				break;

			var community = response.Items.FirstOrDefault(i => i.Slug == slug);

			if (community != null)
				return community;

			// Can't continue to crawl
			if (response.Pagination.NextPage == null)
				break;

			var uri = new Uri(response.Pagination.NextPage);
			var query = HttpUtility.ParseQueryString(uri.Query);
			var nextCursor = query.Get("cursor");

			// Prevent looping
			if (currentCursor == nextCursor)
				break;

			currentCursor = nextCursor;
		} while (currentCursor != null);

		return null;
	}
}
