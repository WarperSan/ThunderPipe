using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
public sealed class CategoryApiClient
{
	private readonly HttpApiClient _client;
	private readonly RequestBuilder _builder;
	private readonly ILogger? _logger;

	public CategoryApiClient(HttpApiClient client, RequestBuilder builder, ILogger? logger = null)
	{
		_client = client;
		_builder = builder;
		_logger = logger;
	}

	/// <summary>
	/// Finds the missing categories in the given community
	/// </summary>
	public async Task<IReadOnlyCollection<Category>> GetMissing(
		IEnumerable<Category> categories,
		Community community,
		CancellationToken ct = default
	)
	{
		var tempBuilder = new RequestBuilder(_builder)
			.Get()
			.ToEndpoint($"api/experimental/community/{community}/category/");

		var categoriesToFind = new Dictionary<string, Category>();
		var visitedPages = new HashSet<string>();

		foreach (var category in categories)
			categoriesToFind[category] = category;

		while (categoriesToFind.Count > 0)
		{
			var request = tempBuilder.Build();

			// Prevent loops
			if (!visitedPages.Add(request.RequestUri!.AbsoluteUri))
				break;

			var response = await _client.SendRequest<Models.Web.GetCategory.Response>(request, ct);

			response.LogErrors(_logger);
			response.EnsureSuccess(out var data);

			foreach (var category in data.Items)
				categoriesToFind.Remove(category.Slug);

			// Can't continue to crawl
			if (data.Pagination.NextPage == null)
				break;

			if (!Uri.TryCreate(data.Pagination.NextPage, UriKind.Absolute, out var uri))
				break;

			tempBuilder = new RequestBuilder(_builder).Get().ToUri(uri);
		}

		return categoriesToFind.Values;
	}
}
