using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the request payload in <see cref="ThunderstoreAPI.ValidateReadme"/>
/// </summary>
internal record ValidateReadmeRequest
{
	/// <summary>
	/// String representation of the manifest encoded with base-64 digits.
	/// </summary>
	[JsonProperty("readme_data")]
	[JsonRequired]
	public required string Data { get; init; }
}

/// <summary>
/// Model used as the response payload in <see cref="ThunderstoreAPI.ValidateReadme"/>
/// </summary>
internal record ValidateReadmeResponse
{
	/// <summary>
	/// Errors related to the data transferred
	/// </summary>
	[JsonProperty("readme_data")]
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
