using Newtonsoft.Json;

namespace ThunderPipe.Models.API.GetDependency;

internal record Response
{
	[JsonProperty("is_active")]
	[JsonRequired]
	public required bool IsActive { get; init; }
}
