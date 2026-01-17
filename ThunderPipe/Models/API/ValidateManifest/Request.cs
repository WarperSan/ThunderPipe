using Newtonsoft.Json;

namespace ThunderPipe.Models.API.ValidateManifest;

internal record Request
{
	/// <summary>
	/// Name of the author
	/// </summary>
	[JsonProperty("namespace")]
	[JsonRequired]
	public required string AuthorName;

	/// <summary>
	/// String representation of the manifest encoded with base-64 digits.
	/// </summary>
	[JsonProperty("manifest_data")]
	[JsonRequired]
	public required string Data { get; init; }
}
