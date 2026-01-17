using Newtonsoft.Json;

namespace ThunderPipe.Models.API.GetCommunity;

internal record Response
{
	public record PaginationModel
	{
		/// <summary>
		/// URL of the next page
		/// </summary>
		[JsonProperty("next_link")]
		public string? NextPage { get; set; }

		/// <summary>
		/// URL of the previous page
		/// </summary>
		[JsonProperty("previous_link")]
		public string? PreviousPage { get; set; }
	}

	public record PageItemModel
	{
		[JsonProperty("identifier")]
		[JsonRequired]
		public required string Slug { get; set; }

		[JsonProperty("name")]
		[JsonRequired]
		public required string Name { get; set; }
	}

	[JsonProperty("pagination")]
	[JsonRequired]
	public required PaginationModel Pagination { get; set; }

	[JsonProperty("results")]
	[JsonRequired]
	public required PageItemModel[] Items { get; set; }
}
