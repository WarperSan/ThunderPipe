using Newtonsoft.Json;

namespace ThunderPipe.DTOs;

/// <summary>
/// Model used as the request payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.SubmitPackage"/>
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
	// ReSharper disable once InconsistentNaming
	public required string UploadUUID { get; set; }
}

/// <summary>
/// Model used as the response payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.SubmitPackage"/>
/// </summary>
internal record SubmitPackageResponse
{
	public record PackageVersionModel
	{
		[JsonProperty("name")]
		[JsonRequired]
		public required string Name { get; set; }

		[JsonProperty("version")]
		[JsonRequired]
		public required string Version { get; set; }

		[JsonProperty("download_url")]
		[JsonRequired]
		// ReSharper disable once InconsistentNaming
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
		// ReSharper disable once InconsistentNaming
		public required string IconURL { get; set; }

		[JsonProperty("dependencies")]
		[JsonRequired]
		public required object[] Dependencies { get; set; }

		[JsonProperty("website_url")]
		[JsonRequired]
		// ReSharper disable once InconsistentNaming
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

	[JsonProperty("package_version")]
	[JsonRequired]
	public required PackageVersionModel Version { get; set; }
}
