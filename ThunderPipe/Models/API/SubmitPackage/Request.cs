using Newtonsoft.Json;

namespace ThunderPipe.Models.API.SubmitPackage;

internal record Request
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
