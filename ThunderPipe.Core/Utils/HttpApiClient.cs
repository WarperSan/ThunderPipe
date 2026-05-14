using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.Web;

namespace ThunderPipe.Core.Utils;

/// <summary>
/// Wrapper around <see cref="HttpClient"/>
/// </summary>
public sealed class HttpApiClient : IDisposable
{
	public HttpApiClient(HttpClient client, ILogger? logger = null)
	{
		_client = client;
		_logger = logger;
	}

	#region Properties

	/// <summary>
	/// <see cref="HttpClient"/> instance used to execute requests
	/// </summary>
	private readonly HttpClient _client;

	/// <summary>
	/// <see cref="ILogger"/> instance used to log client operations
	/// </summary>
	private readonly ILogger? _logger;

	#endregion

	#region Requests

	/// <summary>
	/// Sends the given request
	/// </summary>
	public async Task<HttpResponseMessage> SendRequest(
		HttpRequestMessage request,
		CancellationToken ct
	)
	{
		var response = await _client.SendAsync(request, ct);

		_logger?.LogDebug("Sent request:\n{Request}", response.RequestMessage);
		_logger?.LogDebug("Received response:\n{Response}", response);
		return response;
	}

	/// <summary>
	/// Sends the given request, and parses the returning JSON
	/// </summary>
	public async Task<Response<T>> SendRequest<T>(HttpRequestMessage request, CancellationToken ct)
		where T : class
	{
		var response = await SendRequest(request, ct);
		var content = await response.Content.ReadAsStringAsync(ct);
		_logger?.LogDebug("Received JSON:\n{Json}", content);

		return Response<T>.CreateResponse(response, content);
	}

	#endregion

	/// <inheritdoc />
	public void Dispose()
	{
		_client.Dispose();
	}
}
