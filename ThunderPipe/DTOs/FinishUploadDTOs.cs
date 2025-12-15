using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the request payload in <see cref="ThunderstoreAPI.FinishMultipartUpload"/>
/// </summary>
internal record FinishUploadRequest
{
	/// <summary>
	/// Every part of the upload
	/// </summary>
	[JsonProperty("parts")]
	[JsonRequired]
	public required UploadPartResponse[] Parts { get; set; }
}
