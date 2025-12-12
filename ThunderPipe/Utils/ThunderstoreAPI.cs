using System.Net;
using System.Security.Cryptography;
using System.Text;
using ThunderPipe.Models;

namespace ThunderPipe.Utils;

/// <summary>
/// Class holding the main API calls to Thunderstore
/// </summary>
internal static class ThunderstoreApi
{
	private const string API_EXPERIMENTAL = "api/experimental/";

	/// <summary>
	/// Initiates a multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CreateMultipartUpload.html">Multipart upload initiation</a> step
	/// </remarks>
	public static Task<InitiateUploadResponseModel?> InitiateMultipartUpload(
		string            path,
		RequestBuilder    builder,
		CancellationToken cancellationToken
	)
	{
		var fileInfo = new FileInfo(path);
		var payload = new InitiateUploadRequestModel
		{
			File = Path.GetFileName(path),
			FileSize = fileInfo.Length,
		};
		
		var request = builder
		              .Copy()
		              .Post()
		              .ToEndpoint(API_EXPERIMENTAL + "usermedia/initiate-upload/")
		              .WithJson(payload) 
		              .Build();
		
		return ThunderstoreClient.SendRequest<InitiateUploadResponseModel>(request, cancellationToken);
	}

	/// <summary>
	/// Uploads the single part
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_UploadPart.html">Upload part</a> step
	/// </remarks>
	public static async Task<UploadPartModel?> UploadPart(
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
			md5.TransformBlock(
				bytes,
				0,
				BLOCK_SIZE,
				null,
				0
			);
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

		var request = new RequestBuilder()
		              .ToUrl(url)
		              .Put()
		              .WithContent(content)
		              .Build();

		var response = await ThunderstoreClient.SendRequest(
			request,
			cancellationToken
		);

		var etag = response.Headers.ETag?.Tag;
		
		if (etag == null)
			throw new NullReferenceException("Expected the header 'ETag' to be set.");

		return new UploadPartModel
		{
			ETag = etag,
			PartNumber = id
		};
	}

	/// <summary>
	/// Finishes the multipart upload
	/// </summary>
	/// <remarks>
	/// Internally, this calls the <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_CompleteMultipartUpload.html">Multipart upload completion</a> step
	/// </remarks>
	public static async Task<bool> FinishMultipartUpload(
		string            uuid,
		UploadPartModel[] parts,
		RequestBuilder    builder,
		CancellationToken cancellationToken
	)
	{
		var payload = new FinishUploadRequestModel
		{
			Parts = parts
		};

		var request = builder
		              .Copy()
		              .Post()
		              .ToEndpoint(API_EXPERIMENTAL + $"usermedia/{uuid}/finish-upload/")
		              .WithJson(payload) 
		              .Build();

		var response = await ThunderstoreClient.SendRequest(
			request,
			cancellationToken
		);

		return response.StatusCode == HttpStatusCode.OK;
	}

	public static async Task SubmitPackage(
		string author,
		string community,
		string[] categories,
		bool hasNsfw,
		string uploadUUID,
		RequestBuilder    builder,
		CancellationToken cancellationToken
	)
	{
		var payload = new SubmitPackageRequestModel
		{
			AuthorName = author,
			Categories = [],
			Communities = [community],
			CommunityCategories = new Dictionary<string, string[]>
			{
				[community] = categories
			},
			HasNsfwContent = hasNsfw,
			UploadUUID = uploadUUID
		};
		
		var request = builder
		              .Copy()
		              .Post()
		              .ToEndpoint(API_EXPERIMENTAL + "submission/submit/")
		              .WithJson(payload) 
		              .Build();

		var response = await ThunderstoreClient.SendRequest(
			request,
			cancellationToken
		);
		
		Console.WriteLine(await response.Content.ReadAsStringAsync(cancellationToken));
	}
}