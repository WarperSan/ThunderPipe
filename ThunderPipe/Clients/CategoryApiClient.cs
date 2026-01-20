using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
internal class CategoryApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public CategoryApiClient(RequestBuilder builder, CancellationToken ct)
		: base(builder, ct) { }

	/// <summary>
	/// Finds the missing categories in the given community
	/// </summary>
	public async Task<ISet<string>> GetMissing(string[] slugs, string community)
	{
		var tempBuilder = Builder
			.Copy()
			.Get()
			.ToEndpoint($"api/experimental/community/{community}/category/");

		var slugsToFind = new HashSet<string>(slugs);

		string? currentCursor = null;

		do
		{
			var request = tempBuilder.Copy().SetParameter("cursor", currentCursor).Build();

			var response = await SendRequest<Models.API.GetCategory.Response>(request);

			if (response == null)
				break;

			foreach (var category in response.Items)
				slugsToFind.Remove(category.Slug);

			// Can't continue to crawl
			if (response.Pagination.NextPage == null)
				break;

			var nextCursor = UrlHelper.GetQueryValue(response.Pagination.NextPage, "cursor");

			// Prevent looping
			if (currentCursor == nextCursor)
				break;

			currentCursor = nextCursor;
		} while (slugsToFind.Count > 0 && currentCursor != null);

		return slugsToFind;
	}
}
