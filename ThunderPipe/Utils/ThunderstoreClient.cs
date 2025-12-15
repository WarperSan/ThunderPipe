using System.Net.Http.Headers;

namespace ThunderPipe.Utils;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
internal sealed class ThunderstoreClient : HttpClient
{
	private ThunderstoreClient()
	{
		DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
			nameof(ThunderPipe),
			"1.0.0"
		));
		Timeout = TimeSpan.FromMinutes(5);
	}

	/// <summary>
	/// Sends the given request
	/// </summary>
	public static async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		using var client = new ThunderstoreClient();

		var response = await client.SendAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();

		return response;
	}
}