using System.Security.Cryptography;
using System.Text;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call API endpoints for publishing packages
/// </summary>
internal sealed class PublishApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public PublishApiClient(RequestBuilder builder, HttpClient client, CancellationToken ct)
		: base(builder, client, ct) { }

	/// <summary>
	/// Initiates a multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CreateMultipartUpload.html">Multipart upload initiation</a> step
	/// </remarks>
	public Task<Models.API.InitiateMultipartUpload.Response> InitiateMultipartUpload(string path)
	{
		var fileInfo = new FileInfo(path);

		var payload = new Models.API.InitiateMultipartUpload.Request
		{
			File = Path.GetFileName(path),
			FileSize = fileInfo.Length,
		};

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint("api/experimental/usermedia/initiate-upload/")
			.WithJSON(payload)
			.Build();

		return SendRequest<Models.API.InitiateMultipartUpload.Response>(request);
	}

	/// <summary>
	/// Uploads every part of the file
	/// </summary>
	/// <remarks>
	/// This is simply a helper method to simplify using <see cref="UploadPart"/>
	/// </remarks>
	public async Task<Models.API.UploadPart.Response[]> UploadParts(
		string file,
		Models.API.InitiateMultipartUpload.Response.UploadPartModel[] parts
	)
	{
		var uploadTasks = new List<Task<Models.API.UploadPart.Response>>();

		await using (var stream = File.OpenRead(file))
		{
			foreach (var part in parts)
			{
				stream.Seek(part.Offset, SeekOrigin.Begin);

				var task = UploadPart(stream, part.PartNumber, part.Size, part.Url);

				uploadTasks.Add(task);
			}
		}

		var uploadedParts = await Task.WhenAll(uploadTasks).WaitAsync(CancellationToken);

		return uploadedParts.ToArray();
	}

	/// <summary>
	/// Uploads the single part
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_UploadPart.html">Upload part</a> step
	/// </remarks>
	private async Task<Models.API.UploadPart.Response> UploadPart(
		Stream stream,
		int id,
		int size,
		string url
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
			await chunk.WriteAsync(bytes, CancellationToken);
		}

		var finalBytes = reader.ReadBytes(remainingSize);
		md5.TransformFinalBlock(finalBytes, 0, remainingSize);

		if (md5.Hash == null)
			throw new NullReferenceException($"MD5 hashing failed for part #{id}.");

		var hash = md5.Hash;
		await chunk.WriteAsync(finalBytes, CancellationToken);
		chunk.Position = 0;

		var content = new StreamContent(chunk);
		content.Headers.ContentMD5 = hash;
		content.Headers.ContentLength = size;

		var uri = new Uri(url, UriKind.Absolute);
		var request = new RequestBuilder().ToUri(uri).Put().WithContent(content).Build();

		var response = await SendRequest(request);

		var etag = response.Headers.ETag?.Tag;

		if (etag == null)
			throw new NullReferenceException("Expected the header 'ETag' to be set.");

		return new Models.API.UploadPart.Response { ETag = etag, PartNumber = id };
	}

	/// <summary>
	/// Aborts the multipart upload
	/// </summary>
	public async Task AbortMultipartUpload(string uuid)
	{
		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint($"api/experimental/usermedia/{uuid}/abort-upload/")
			.Build();

		await SendRequest(request);
	}

	/// <summary>
	/// Finishes the multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CompleteMultipartUpload.html">Multipart upload completion</a> step
	/// </remarks>
	public async Task<bool> FinishMultipartUpload(
		string uuid,
		Models.API.UploadPart.Response[] parts
	)
	{
		var payload = new Models.API.FinishMultipartUpload.Request { Parts = parts };

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint($"api/experimental/usermedia/{uuid}/finish-upload/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest(request);

		return response.IsSuccessStatusCode;
	}

	/// <summary>
	/// Submits the package
	/// </summary>
	public Task<Models.API.SubmitPackage.Response> SubmitPackage(
		string author,
		string community,
		string[] categories,
		bool hasNsfw,
		string uploadUUID
	)
	{
		var payload = new Models.API.SubmitPackage.Request
		{
			AuthorName = author,
			Communities = [community],
			CommunityCategories = new Dictionary<string, string[]> { [community] = categories },
			HasNsfwContent = hasNsfw,
			UploadUUID = uploadUUID,
		};

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint("api/experimental/submission/submit/")
			.WithJSON(payload)
			.Build();

		return SendRequest<Models.API.SubmitPackage.Response>(request);
	}
}
