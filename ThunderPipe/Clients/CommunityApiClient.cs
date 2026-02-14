using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call API endpoints related to communities
/// </summary>
internal sealed class CommunityApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public CommunityApiClient(RequestBuilder builder, HttpClient client, CancellationToken ct)
		: base(builder, client, ct) { }

	/// <summary>
	/// Checks if a community with the given slug exists
	/// </summary>
	public async Task<bool> Exists(string slug)
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

			var response = await SendRequest<Models.API.GetCommunity.Response>(request);

			var community = response.Items.FirstOrDefault(i => i.Slug == slug);

			if (community != null)
			{
				wasCommunityFound = true;
				break;
			}

			// Can't continue to crawl
			if (response.Pagination.NextPage == null)
				break;

			if (!Uri.TryCreate(response.Pagination.NextPage, UriKind.Absolute, out var uri))
				break;

			tempBuilder = new RequestBuilder(Builder).Get().ToUri(uri);
		}

		return wasCommunityFound;
	}
}
