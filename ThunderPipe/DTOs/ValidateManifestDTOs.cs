using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the request payload in <see cref="ThunderstoreAPI.ValidateManifest"/>
/// </summary>
internal record ValidateManifestRequest
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

/// <summary>
/// Model used as the response payload in <see cref="ThunderstoreAPI.ValidateManifest"/>
/// </summary>
internal record ValidateManifestResponse
{
	/// <summary>
	/// Errors related to the data transferred
	/// </summary>
	[JsonProperty("manifest_data")]
	public string[]? DataErrors { get; init; }

	/// <summary>
	/// Errors related to the namespace
	/// </summary>
	[JsonProperty("namespace")]
	public string[]? NamespaceErrors { get; init; }

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
