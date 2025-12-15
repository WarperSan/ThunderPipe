using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the request payload in <see cref="ThunderstoreAPI.SubmitPackage"/>
/// </summary>
internal record SubmitPackageRequest
{
	/// <summary>
	/// Name of the author
	/// </summary>
	[JsonProperty("author_name")]
	[JsonRequired]
	public required string AuthorName { get; set; }

	/// <summary>
	/// Communities to upload to
	/// </summary>
	[JsonProperty("communities")]
	[JsonRequired]
	public required string[] Communities { get; set; }

	/// <summary>
	/// Categories to use for each community
	/// </summary>
	[JsonProperty("community_categories")]
	[JsonRequired]
	public required Dictionary<string, string[]> CommunityCategories { get; set; }

	/// <summary>
	/// If this package contains NSFW content
	/// </summary>
	[JsonProperty("has_nsfw_content")]
	[JsonRequired]
	public required bool HasNsfwContent { get; set; }

	/// <summary>
	/// Unique identifier of the uploaded file
	/// </summary>
	[JsonProperty("upload_uuid")]
	[JsonRequired]
	public required string UploadUUID { get; set; }
}

/// <summary>
/// Model used as the response payload in <see cref="ThunderstoreAPI.SubmitPackage"/>
/// </summary>
internal record SubmitPackageResponse
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

		// These fields are not used by this tool
#if false
		[JsonProperty("namespace")]
		[JsonRequired]
		public required string Namespace { get; set; }

		[JsonProperty("full_name")]
		[JsonRequired]
		public required string Fullname { get; set; }

		[JsonProperty("description")]
		[JsonRequired]
		public required string Description { get; set; }
		
		[JsonProperty("icon")]
		[JsonRequired]
		public required string IconURL { get; set; }

		// TODO: Add proper parsing of dependencies
		[JsonProperty("dependencies")]
		[JsonRequired]
		public required object[] Dependencies { get; set; }

		[JsonProperty("website_url")]
		[JsonRequired]
		public required string WebsiteURL { get; set; }

		[JsonProperty("is_active")]
		[JsonRequired]
		public required bool IsActive { get; set; }

		[JsonProperty("date_created")]
		[JsonRequired]
		public required DateTime CreatedAt { get; set; }

		[JsonProperty("downloads")]
		[JsonRequired]
		public required int DownloadCount { get; set; }
#endif
	}

	/// <summary>
	/// Version information about this package
	/// </summary>
	[JsonProperty("package_version")]
	[JsonRequired]
	public required PackageVersionModel Version { get; set; }

	// These fields are not used by this tool
#if false
	// TODO: Add proper parsing of available communities
	[JsonProperty("available_communities")]
	[JsonRequired]
	public required object[] Communities { get; set; }
#endif
}
