using Newtonsoft.Json;

namespace ThunderPipe.Models;

/// <summary>
/// Model used as the request payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.SubmitPackage"/>
/// </summary>
internal record SubmitPackageRequestModel
{
	[JsonProperty("author_name")]
	[JsonRequired]
	public required string AuthorName { get; set; }

	[JsonProperty("categories")]
	[JsonRequired]
	public required string[] Categories { get; set; }

	[JsonProperty("communities")]
	[JsonRequired]
	public required string[] Communities { get; set; }

	[JsonProperty("community_categories")]
	[JsonRequired]
	public required Dictionary<string, string[]> CommunityCategories { get; set; }

	[JsonProperty("has_nsfw_content")]
	[JsonRequired]
	public required bool HasNsfwContent { get; set; }

	[JsonProperty("upload_uuid")]
	[JsonRequired]
	// ReSharper disable once InconsistentNaming
	public required string UploadUUID { get; set; }
}