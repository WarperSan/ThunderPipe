using Newtonsoft.Json;

namespace ThunderPipe.Core.DTOs.GetDependency;

internal record Response
{
	[JsonProperty("is_active")]
	[JsonRequired]
	public required bool IsActive { get; init; }
}
