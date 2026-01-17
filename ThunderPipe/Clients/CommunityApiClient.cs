using System.Web;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call API endpoints related to communities
/// </summary>
internal sealed class CommunityApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public CommunityApiClient(RequestBuilder builder, CancellationToken ct)
		: base(builder, ct) { }

	/// <summary>
	/// Finds the community with the slug
	/// </summary>
	public async Task<Models.API.GetCommunity.Response.PageItemModel?> FindCommunity(string slug)
	{
		var tempBuilder = Builder.Copy().Get().ToEndpoint(API_EXPERIMENTAL + "community/");

		string? currentCursor = null;

		do
		{
			var request = tempBuilder.Copy().SetParameter("cursor", currentCursor).Build();

			var response = await SendRequest<Models.API.GetCommunity.Response>(request);

			if (response == null)
				break;

			var community = response.Items.FirstOrDefault(i => i.Slug == slug);

			if (community != null)
				return community;

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
		} while (currentCursor != null);

		return null;
	}
}
