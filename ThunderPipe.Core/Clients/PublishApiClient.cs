using System.Security.Cryptography;
using System.Text;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Models.Web.MultipartUpload;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints for publishing packages
/// </summary>
public sealed class PublishApiClient : ThunderstoreClient
{
	// TODO: Try to limit the amount of Models.Web returned

	/// <summary>
	/// Initiates a multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CreateMultipartUpload.html">Multipart upload initiation</a> step
	/// </remarks>
	public async Task<UploadSession> InitiateMultipartUpload(
		string path,
		IFileSystem fileSystem,
		string token,
		CancellationToken ct = default
	)
	{
		var payload = new Models.Web.InitiateMultipartUpload.Request
		{
			File = fileSystem.GetName(path),
			FileSize = fileSystem.GetSize(path),
		};

		var request = new RequestBuilder(Builder)
			.Post()
			.WithAuth(token)
			.ToEndpoint("api/experimental/usermedia/initiate-upload/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.Web.InitiateMultipartUpload.Response>(request, ct);

		var parts = response.UploadParts.Select(p => new UploadPartDescriptor
		{
			Id = p.PartNumber,
			UploadURL = new Uri(p.Url),
			Offset = p.Offset,
			Size = p.Size,
		});

		return new UploadSession
		{
			UUID = response.FileMetadata.UUID,
			Parts = parts.ToArray().AsReadOnly(),
		};
	}

	/// <summary>
	/// Uploads every part of the file
	/// </summary>
	/// <remarks>
	/// This is simply a helper method to simplify using <see cref="UploadPart"/>
	/// </remarks>
	public async Task<IReadOnlyCollection<UploadPart>> UploadParts(
		string file,
		IEnumerable<UploadPartDescriptor> parts,
		IFileSystem fileSystem,
		CancellationToken ct = default
	)
	{
		var uploadTasks = new List<Task<UploadPart>>();
		UploadPart[] uploadedParts;

		await using (var stream = fileSystem.OpenRead(file))
		{
			foreach (var part in parts)
			{
				stream.Seek(part.Offset, SeekOrigin.Begin);

				var task = UploadPart(stream, part.Id, part.Size, part.UploadURL, ct);

				uploadTasks.Add(task);
			}

			uploadedParts = await Task.WhenAll(uploadTasks).WaitAsync(ct);
		}

		return uploadedParts.AsReadOnly();
	}

	/// <summary>
	/// Uploads the single part
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_UploadPart.html">Upload part</a> step
	/// </remarks>
	private async Task<UploadPart> UploadPart(
		Stream stream,
		int id,
		int size,
		Uri url,
		CancellationToken ct
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
			await chunk.WriteAsync(bytes, ct);
		}

		var finalBytes = reader.ReadBytes(remainingSize);
		md5.TransformFinalBlock(finalBytes, 0, remainingSize);

		if (md5.Hash == null)
			throw new NullReferenceException($"MD5 hashing failed for part #{id}.");

		var hash = md5.Hash;
		await chunk.WriteAsync(finalBytes, ct);
		chunk.Position = 0;

		var content = new StreamContent(chunk);
		content.Headers.ContentMD5 = hash;
		content.Headers.ContentLength = size;

		var request = new RequestBuilder().ToUri(url).Put().WithContent(content).Build();

		var response = await SendRequest(request, ct);

		var etag = response.Headers.ETag?.Tag;

		if (etag == null)
			throw new NullReferenceException("Expected the header 'ETag' to be set.");

		return new UploadPart { Id = id, ETag = etag };
	}

	/// <summary>
	/// Aborts the multipart upload
	/// </summary>
	public async Task AbortMultipartUpload(
		string uuid,
		string token,
		CancellationToken ct = default
	)
	{
		var request = new RequestBuilder(Builder)
			.Post()
			.WithAuth(token)
			.ToEndpoint($"api/experimental/usermedia/{uuid}/abort-upload/")
			.Build();

		await SendRequest(request, ct);
	}

	/// <summary>
	/// Finishes the multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CompleteMultipartUpload.html">Multipart upload completion</a> step
	/// </remarks>
	public async Task<bool> FinishMultipartUpload(
		string uuid,
		IEnumerable<UploadPart> parts,
		string token,
		CancellationToken ct = default
	)
	{
		var payload = new Models.Web.FinishMultipartUpload.Request
		{
			Parts = parts
				.Select(p => new Models.Web.UploadPart.Response
				{
					PartNumber = p.Id,
					ETag = p.ETag,
				})
				.ToArray(),
		};

		var request = new RequestBuilder(Builder)
			.Post()
			.WithAuth(token)
			.ToEndpoint($"api/experimental/usermedia/{uuid}/finish-upload/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest(request, ct);

		return response.IsSuccessStatusCode;
	}

	/// <summary>
	/// Submits the package
	/// </summary>
	public async Task<Package> SubmitPackage(
		Team team,
		IEnumerable<Community> communities,
		IDictionary<Community, IEnumerable<Category>> categories,
		bool hasNsfw,
		string sessionUUID,
		string token,
		CancellationToken ct = default
	)
	{
		var categoriesPerCommunity = categories.ToDictionary(
			kvp => (string)kvp.Key,
			kvp => kvp.Value.Select(c => (string)c).ToArray()
		);

		var payload = new Models.Web.SubmitPackage.Request
		{
			AuthorName = team,
			Communities = communities.Select(c => (string)c).ToArray(),
			CommunityCategories = categoriesPerCommunity.ToDictionary(),
			HasNsfwContent = hasNsfw,
			UploadUUID = sessionUUID,
		};

		var request = new RequestBuilder(Builder)
			.Post()
			.WithAuth(token)
			.ToEndpoint("api/experimental/submission/submit/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.Web.SubmitPackage.Response>(request, ct);

		return new Package(
			response.Version.Name,
			response.Version.Version,
			response.Version.DownloadURL
		);
	}
}
