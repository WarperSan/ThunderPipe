using Newtonsoft.Json;

namespace ThunderPipe.Core.Models.API;

public sealed record PackageManifest
{
	[JsonProperty("name")]
	public required PackageName Name;

	[JsonProperty("version_number")]
	public required PackageVersion Version;

	[JsonProperty("description")]
	public string Description = "";

	[JsonProperty("website_url")]
	public string Website = "";

	[JsonProperty("dependencies")]
	public PackageDependency[] Dependencies = [];

	/// <summary>
	/// Checks if the underlying JSON is valid
	/// </summary>
	public bool IsValid()
	{
		if (Dependencies.Any(d => !d.IsValid()))
			return false;

		return Name.IsValid() && Version.IsValid();
	}
}
