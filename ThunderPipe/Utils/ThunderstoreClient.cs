namespace ThunderPipe.Utils;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
internal sealed class ThunderstoreClient : HttpClient
{
	private ThunderstoreClient()
	{
		Timeout = TimeSpan.FromMinutes(5);
	}

	/// <summary>
	/// Sends the given request
	/// </summary>
	public static async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		using var client = new ThunderstoreClient();
		
		var response = await client.SendAsync(request, cancellationToken);
		//response.EnsureSuccessStatusCode();

		return response;
	}

	/// <summary>
	/// Sends the given request, and returns the given JSON response
	/// </summary>
	public static async Task<T?> SendRequest<T>(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var response = await SendRequest(request, cancellationToken);
		var content = await response.Content.ReadAsStringAsync(cancellationToken);

		return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
	}
}