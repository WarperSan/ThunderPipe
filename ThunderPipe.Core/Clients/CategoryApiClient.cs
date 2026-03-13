using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
internal sealed class CategoryApiClient : ThunderstoreClient
{
	/// <summary>
	/// Finds the missing categories in the given community
	/// </summary>
	public async Task<ISet<string>> GetMissing(string[] slugs, string community)
	{
		var tempBuilder = new RequestBuilder(Builder)
			.Get()
			.ToEndpoint($"api/experimental/community/{community}/category/");

		var slugsToFind = new HashSet<string>(slugs);
		var visitedPages = new HashSet<string>();

		while (slugsToFind.Count > 0)
		{
			var request = tempBuilder.Build();

			// Prevent loops
			if (!visitedPages.Add(request.RequestUri!.AbsoluteUri))
				break;

			var response = await SendRequest<Models.Web.GetCategory.Response>(request);

			foreach (var category in response.Items)
				slugsToFind.Remove(category.Slug);

			// Can't continue to crawl
			if (response.Pagination.NextPage == null)
				break;

			if (!Uri.TryCreate(response.Pagination.NextPage, UriKind.Absolute, out var uri))
				break;

			tempBuilder = new RequestBuilder(Builder).Get().ToUri(uri);
		}

		return slugsToFind;
	}
}
