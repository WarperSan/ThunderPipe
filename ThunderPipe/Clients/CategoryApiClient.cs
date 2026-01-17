using System.Web;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
internal sealed class CategoryApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public CategoryApiClient(RequestBuilder builder, CancellationToken ct)
		: base(builder, ct) { }

	/// <summary>
	/// Finds the categories with the slugs in the community
	/// </summary>
	public async Task<
		Dictionary<string, Models.API.GetCategory.Response.PageItemModel>
	> FindCategories(string[] slugs, string community)
	{
		var tempBuilder = Builder
			.Copy()
			.Get()
			.ToEndpoint(API_EXPERIMENTAL + $"community/{community}/category/");

		var slugsHash = new HashSet<string>(slugs);
		var categories = new Dictionary<string, Models.API.GetCategory.Response.PageItemModel>();
		string? currentCursor = null;

		do
		{
			var request = tempBuilder.Copy().SetParameter("cursor", currentCursor).Build();

			var response = await SendRequest<Models.API.GetCategory.Response>(request);

			if (response == null)
				break;

			foreach (var category in response.Items)
			{
				if (!slugsHash.Contains(category.Slug))
					continue;

				categories[category.Slug] = category;
			}

			// Can't continue to crawl
			if (response.Pagination.NextPage == null)
				break;

			var uri = new Uri(response.Pagination.NextPage);
			var query = HttpUtility.ParseQueryString(uri.Query);
			var nextCursor = query.Get("cursor");

			// Prevent looping
			if (currentCursor == nextCursor)
				break;

			currentCursor = nextCursor;
		} while (categories.Count < slugs.Length && currentCursor != null);

		return categories;
	}
}
