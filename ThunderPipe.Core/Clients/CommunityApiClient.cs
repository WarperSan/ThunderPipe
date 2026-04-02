using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to communities
/// </summary>
public sealed class CommunityApiClient : ThunderstoreClient
{
	/// <summary>
	/// Finds the missing communities
	/// </summary>
	public async Task<IReadOnlyCollection<Community>> GetMissing(
		IEnumerable<Community> communities,
		CancellationToken ct = default
	)
	{
		var tempBuilder = new RequestBuilder(Builder)
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

			var response = await SendRequest<Models.Web.GetCommunity.Response>(request, ct);

			response.LogErrors(Logger);
			response.EnsureSuccess(out var data);

			foreach (var community in data.Items)
				communitiesToFind.Remove(community.Slug);

			// Can't continue to crawl
			if (data.Pagination.NextPage == null)
				break;

			if (!Uri.TryCreate(data.Pagination.NextPage, UriKind.Absolute, out var uri))
				break;

			tempBuilder = new RequestBuilder(Builder).Get().ToUri(uri);
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
