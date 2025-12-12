using Newtonsoft.Json;

namespace ThunderPipe.Models;

/// <summary>
/// Model used as the request payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.FinishMultipartUpload"/>
/// </summary>
internal record FinishUploadRequestModel
{
	[JsonProperty("parts")]
	[JsonRequired]
	public required UploadPartModel[] Parts { get; set; }
}