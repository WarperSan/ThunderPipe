using Newtonsoft.Json;

namespace ThunderPipe.Core.Models.Web.ValidateReadme;

internal record Request
{
	/// <summary>
	/// String representation of the manifest encoded with base-64 digits.
	/// </summary>
	[JsonProperty("readme_data")]
	[JsonRequired]
	public required string Data { get; init; }
}
