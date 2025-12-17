using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the request payload in <see cref="ThunderstoreAPI.ValidateIcon"/>
/// </summary>
internal record ValidateIconRequest
{
	/// <summary>
	/// String representation of the icon encoded with base-64 digits.
	/// </summary>
	[JsonProperty("icon_data")]
	[JsonRequired]
	public required string Data { get; init; }
}

/// <summary>
/// Model used as the response payload in <see cref="ThunderstoreAPI.ValidateIcon"/>
/// </summary>
internal record ValidateIconResponse
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
