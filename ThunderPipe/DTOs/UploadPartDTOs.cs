using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the response payload in <see cref="ThunderstoreAPI.UploadPart"/>
/// </summary>
[SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names")]
internal record UploadPartResponse
{
	/// <summary>
	/// Entity tag for the uploaded part
	/// </summary>
	[JsonProperty("ETag")]
	[JsonRequired]
	public required string ETag { get; set; }

	/// <summary>
	/// Part number of part being uploaded
	/// </summary>
	[JsonProperty("PartNumber")]
	[JsonRequired]
	public required int PartNumber { get; set; }
}
