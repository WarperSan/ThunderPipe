using Newtonsoft.Json;

namespace ThunderPipe.Models.API.GetCategory;

internal record Response
{
	public record PaginationModel
	{
		/// <summary>
		/// URL of the next page
		/// </summary>
		[JsonProperty("next_link")]
		public string? NextPage { get; set; }
	}

	public record PageItemModel
	{
		[JsonProperty("slug")]
		[JsonRequired]
		public required string Slug { get; set; }
	}

	[JsonProperty("pagination")]
	[JsonRequired]
	public required PaginationModel Pagination { get; set; }

	[JsonProperty("results")]
	[JsonRequired]
	public required PageItemModel[] Items { get; set; }
}
