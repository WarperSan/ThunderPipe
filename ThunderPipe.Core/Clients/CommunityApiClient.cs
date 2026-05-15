using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to communities
/// </summary>
public sealed class CommunityApiClient
{
	private readonly HttpApiClient _client;
	private readonly RequestBuilder _builder;
	private readonly ILogger? _logger;

	public CommunityApiClient(HttpApiClient client, RequestBuilder builder, ILogger? logger = null)
	{
		_client = client;
		_builder = builder;
		_logger = logger;
	}

	/// <summary>
	/// Finds the missing communities
	/// </summary>
	public async Task<IReadOnlyCollection<Community>> GetMissing(
		IEnumerable<Community> communities,
		CancellationToken ct = default
	)
	{
		var tempBuilder = new RequestBuilder(_builder)
			.Get()
			.ToEndpoint("api/experimental/community/");

		var communitiesToFind = new Dictionary<string, Community>();
		var visitedPages = new HashSet<string>();

		foreach (var community in communities)
			communitiesToFind[community] = community;

		while (communitiesToFind.Count > 0)
		{
			var request = tempBuilder.Build();

			// Prevent loopstrue
			if (!visitedPages.Add(request.RequestUri!.AbsoluteUri))
				break;

			var response = await _client.SendRequest<Models.Web.GetCommunity.Response>(request, ct);

			response.LogErrors(_logger);
			response.EnsureSuccess(out var data);

			foreach (var community in data.Items)
				communitiesToFind.Remove(community.Slug);

			// Can't continue to crawl
			if (data.Pagination.NextPage == null)
				break;

			if (!Uri.TryCreate(data.Pagination.NextPage, UriKind.Absolute, out var uri))
				break;

			tempBuilder = new RequestBuilder(_builder).Get().ToUri(uri);
		}

		return communitiesToFind.Values;
	}

	/// <summary>
	/// Checks if the given community exists
	/// </summary>
	public async Task<bool> Exists(Community community, CancellationToken ct = default)
	{
		var missingCommunities = await GetMissing([community], ct);

		return missingCommunities.Count == 0;
	}
}
