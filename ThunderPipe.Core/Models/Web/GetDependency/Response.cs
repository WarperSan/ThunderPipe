using Newtonsoft.Json;

namespace ThunderPipe.Core.Models.Web.GetDependency;

internal record Response
{
	[JsonProperty("is_active")]
	[JsonRequired]
	public required bool IsActive { get; init; }
}
