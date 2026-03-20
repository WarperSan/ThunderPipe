using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to communities
/// </summary>
public sealed class CommunityApiClient : ThunderstoreClient
{
	/// <summary>
	/// Checks if the given community exists
	/// </summary>
	public async Task<bool> Exists(Community community, CancellationToken ct = default)
	{
		var tempBuilder = new RequestBuilder(Builder)
			.Get()
			.ToEndpoint("api/experimental/community/");

		var visitedPages = new HashSet<string>();
		var wasCommunityFound = false;

		while (true)
		{
			var request = tempBuilder.Build();

			// Prevent loops
			if (!visitedPages.Add(request.RequestUri!.AbsoluteUri))
				break;

			var response = await SendRequest<Models.Web.GetCommunity.Response>(request, ct);

			response.LogErrors(Logger);
			response.EnsureSuccess(out var data);

			var rawCommunity = data.Items.FirstOrDefault(i => i.Slug == community);

			if (rawCommunity != null)
			{
				wasCommunityFound = true;
				break;
			}

			// Can't continue to crawl
			if (data.Pagination.NextPage == null)
				break;

			if (!Uri.TryCreate(data.Pagination.NextPage, UriKind.Absolute, out var uri))
				break;

			tempBuilder = new RequestBuilder(Builder).Get().ToUri(uri);
		}

		return wasCommunityFound;
	}
}
