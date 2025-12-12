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
}