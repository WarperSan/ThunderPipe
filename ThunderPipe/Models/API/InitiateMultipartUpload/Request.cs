using Newtonsoft.Json;

namespace ThunderPipe.Models.API.InitiateMultipartUpload;

internal record Request
{
	/// <summary>
	/// Name of the file uploaded to the Amazon S3 bucket
	/// </summary>
	/// <remarks>
	/// It refers to <a href="http://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html">this</a>
	/// </remarks>
	[JsonProperty("filename")]
	[JsonRequired]
	public required string File { get; set; }

	/// <summary>
	/// Size of the file uploaded in bytes
	/// </summary>
	[JsonProperty("file_size_bytes")]
	[JsonRequired]
	public required long FileSize { get; set; }
}
