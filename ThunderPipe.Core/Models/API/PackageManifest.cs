using Newtonsoft.Json;

namespace ThunderPipe.Core.Models.API;

public sealed record PackageManifest
{
	[JsonProperty("name")]
	public required PackageName Name { get; init; }

	[JsonProperty("version_number")]
	public required PackageVersion Version { get; init; }

	[JsonProperty("description")]
	public string Description { get; init; } = "";

	[JsonProperty("website_url")]
	public string Website { get; init; } = "";

	[JsonProperty("dependencies")]
	public PackageDependency[] Dependencies { get; init; } = [];
}
