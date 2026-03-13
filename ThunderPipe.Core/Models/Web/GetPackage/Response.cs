using Newtonsoft.Json;

namespace ThunderPipe.Core.Models.Web.GetPackage;

internal record Response
{
	public record LatestModel
	{
		[JsonProperty("version_number")]
		[JsonRequired]
		public required string Version { get; set; }
	}

	[JsonProperty("latest")]
	[JsonRequired]
	public required LatestModel LatestPackage { get; init; }
}
