using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace ThunderPipe.Models;

/// <summary>
/// Model used as the response payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.UploadPart"/>
/// </summary>
[SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names")]
internal record UploadPartModel
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