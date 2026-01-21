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
		var tempBuilder = Builder.Copy().Get().ToEndpoint("api/experimental/community/");

		string? currentCursor = null;

		do
		{
			var request = tempBuilder.Copy().SetParameter("cursor", currentCursor).Build();

			var response = await SendRequest<Models.API.GetCommunity.Response>(request);

			var community = response.Items.FirstOrDefault(i => i.Slug == slug);

			if (community != null)
				return true;

			// Can't continue to crawl
			if (response.Pagination.NextPage == null)
				break;

			var nextCursor = UrlHelper.GetQueryValue(response.Pagination.NextPage, "cursor");

			// Prevent looping
			if (currentCursor == nextCursor)
				break;

			currentCursor = nextCursor;
		} while (currentCursor != null);

		return false;
	}
}
