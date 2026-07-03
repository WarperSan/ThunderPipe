using Newtonsoft.Json;

namespace ThunderPipe.Core.DTOs.ValidateManifest;

internal record Response
{
	/// <summary>
	/// Validity of the data
	/// </summary>
	[JsonProperty("success")]
	public required bool Valid { get; init; }
}
