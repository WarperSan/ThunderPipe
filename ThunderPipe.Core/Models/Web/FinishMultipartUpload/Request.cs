using Newtonsoft.Json;

namespace ThunderPipe.Core.Models.Web.FinishMultipartUpload;

internal record Request
{
	/// <summary>
	/// Every part of the upload
	/// </summary>
	[JsonProperty("parts")]
	[JsonRequired]
	public required UploadPart.Response[] Parts { get; set; }
}
