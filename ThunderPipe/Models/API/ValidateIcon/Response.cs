using Newtonsoft.Json;

namespace ThunderPipe.Models.API.ValidateIcon;

internal record Response
{
	/// <summary>
	/// Errors related to the data transferred
	/// </summary>
	[JsonProperty("icon_data")]
	public string[]? DataErrors { get; init; }

	/// <summary>
	/// Errors related to the data validity
	/// </summary>
	[JsonProperty("non_field_errors")]
	public string[]? ValidationErrors { get; init; }

	/// <summary>
	/// Validity of the data
	/// </summary>
	[JsonProperty("success")]
	public bool? Valid { get; init; }
}
