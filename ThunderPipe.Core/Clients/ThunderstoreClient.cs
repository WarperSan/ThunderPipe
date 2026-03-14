using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
public abstract class ThunderstoreClient : IDisposable
{
	#region Properties

	private HttpClient _client = null!;
	private RequestBuilder _builder = null!;

	/// <summary>
	/// Default <see cref="RequestBuilder"/> for this client
	/// </summary>
	public RequestBuilder Builder
	{
		protected get => _builder;
		set => _builder = new RequestBuilder(value);
	}

	/// <summary>
	/// <see cref="HttpClient"/> instance used for requests
	/// </summary>
	public HttpClient Client
	{
		set
		{
			_client = value;

			_client.DefaultRequestHeaders.UserAgent.Add(
				new ProductInfoHeaderValue(Metadata.GUID, Metadata.VERSION)
			);
			_client.Timeout = TimeSpan.FromMinutes(5);
		}
	}

	/// <summary>
	/// <see cref="ILogger"/> instead used for operations
	/// </summary>
	public ILogger? Logger { protected get; set; }

	#endregion

	protected ThunderstoreClient()
	{
		Builder = new RequestBuilder();
		Client = new HttpClient();
		Logger = null;
	}

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
	/// Sends the given request, and returns the JSON response
	/// </summary>
	protected async Task<T> SendRequest<T>(HttpRequestMessage request, CancellationToken ct)
	{
		var response = await SendRequest(request, ct);
		return await ParseJson<T>(response, ct);
	}

	/// <summary>
	/// Parses the JSON content of the given response
	/// </summary>
	protected async Task<T> ParseJson<T>(HttpResponseMessage response, CancellationToken ct)
	{
		var content = await response.Content.ReadAsStringAsync(ct);
		Logger?.LogDebug("Received JSON:\n{Json}", content);

		T? json;

		try
		{
			json = JsonConvert.DeserializeObject<T>(content);
		}
		catch (JsonException e)
		{
			throw new InvalidOperationException(
				$"Failed to deserialize the response: \n\n{content}",
				e
			);
		}

		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (json == null)
			throw new NullReferenceException(
				$"Failed to parse the response's payload as '{nameof(T)}'"
			);

		return json;
	}

	#endregion

	/// <inheritdoc />
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		_client.Dispose();
	}
}
