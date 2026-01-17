using Newtonsoft.Json;

namespace ThunderPipe.Models.API.FinishMultipartUpload;

internal record Request
{
	/// <summary>
	/// Every part of the upload
	/// </summary>
	[JsonProperty("parts")]
	[JsonRequired]
	public required UploadPart.Response[] Parts { get; set; }
}
