using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
internal abstract class ThunderstoreClient : IDisposable
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
	/// Token used to cancel operations
	/// </summary>
	public CancellationToken CancellationToken { protected get; set; }

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

	#endregion

	protected ThunderstoreClient()
	{
		Builder = new RequestBuilder();
		CancellationToken = CancellationToken.None;
		Client = new HttpClient();
	}

	#region Requests

	/// <summary>
	/// Sends the given request
	/// </summary>
	protected async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
	{
		return await _client.SendAsync(request, CancellationToken);
	}

	/// <summary>
	/// Sends the given request, and returns the JSON response
	/// </summary>
	protected async Task<T> SendRequest<T>(HttpRequestMessage request)
	{
		var response = await SendRequest(request);
		var content = await response.Content.ReadAsStringAsync(CancellationToken);

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
		_client.Dispose();
	}
}
