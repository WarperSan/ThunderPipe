using Newtonsoft.Json;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the request payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.FinishMultipartUpload"/>
/// </summary>
internal record FinishUploadRequest
{
	[JsonProperty("parts")]
	[JsonRequired]
	public required UploadPartResponse[] Parts { get; set; }
}