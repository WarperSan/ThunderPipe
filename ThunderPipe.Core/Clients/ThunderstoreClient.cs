using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.Web;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
public abstract class ThunderstoreClient : IDisposable
{
	protected ThunderstoreClient()
	{
		Builder = new RequestBuilder();
		Client = new HttpClient();
		Logger = null;
	}

	#region Properties

	private HttpClient _client = null!;
	private RequestBuilder _builder = null!;

	/// <summary>
	/// <see cref="RequestBuilder"/> that all requests are templated from
	/// </summary>
	public RequestBuilder Builder
	{
		protected get => _builder;
		set => _builder = new RequestBuilder(value);
	}

	/// <summary>
	/// <see cref="HttpClient"/> instance used to execute requests
	/// </summary>
	public HttpClient Client
	{
		set
		{
			_client = value;

			_client.DefaultRequestHeaders.UserAgent.Add(
				new ProductInfoHeaderValue(Metadata.GUID, Metadata.VERSION)
			);
		}
	}

	/// <summary>
	/// <see cref="ILogger"/> instance used to log client operations
	/// </summary>
	public ILogger? Logger { protected get; set; }

	#endregion

	#region Requests

	/// <summary>
	/// Sends the given request
	/// </summary>
	protected async Task<HttpResponseMessage> SendRequest(
		HttpRequestMessage request,
		CancellationToken ct
	)
	{
		Logger?.LogDebug("Sending request:\n{Request}", request);
		var response = await _client.SendAsync(request, ct);

		Logger?.LogDebug("Received response:\n{Response}", response);
		return response;
	}

	/// <summary>
	/// Sends the given request, and parses the returning JSON
	/// </summary>
	protected async Task<Response<T>> SendRequest<T>(
		HttpRequestMessage request,
		CancellationToken ct
	)
		where T : class
	{
		var response = await SendRequest(request, ct);
		var content = await response.Content.ReadAsStringAsync(ct);
		Logger?.LogDebug("Received JSON:\n{Json}", content);

		return Response<T>.CreateResponse(response, content);
	}

	#endregion

	/// <inheritdoc />
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		_client.Dispose();
	}
}
