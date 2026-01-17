using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.Models.API.SubmitPackage;

internal record Response
{
	/// <summary>
	/// Model used to represent a package's version in <see cref="ThunderstoreAPI.SubmitPackage"/>
	/// </summary>
	public record PackageVersionModel
	{
		/// <summary>
		/// Name of the package
		/// </summary>
		[JsonProperty("name")]
		[JsonRequired]
		public required string Name { get; set; }

		/// <summary>
		/// Version of the package
		/// </summary>
		[JsonProperty("version_number")]
		[JsonRequired]
		public required string Version { get; set; }

		/// <summary>
		/// URL to download the package
		/// </summary>
		[JsonProperty("download_url")]
		[JsonRequired]
		public required string DownloadURL { get; set; }
	}

	/// <summary>
	/// Version information about this package
	/// </summary>
	[JsonProperty("package_version")]
	[JsonRequired]
	public required PackageVersionModel Version { get; set; }
}
