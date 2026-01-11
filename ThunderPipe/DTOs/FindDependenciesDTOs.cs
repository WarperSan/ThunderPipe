using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the response payload in <see cref="ThunderstoreAPI.FindDependencies"/>
/// </summary>
internal record FindDependenciesResponse
{
	[JsonProperty("is_active")]
	[JsonRequired]
	public required bool IsActive { get; init; }
}
