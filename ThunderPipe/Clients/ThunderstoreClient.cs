using System.Net.Http.Headers;
using Newtonsoft.Json;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
internal abstract class ThunderstoreClient : IDisposable
{
	#region Properties

	private HttpClient? _client;

	/// <summary>
	/// Default <see cref="RequestBuilder"/> for this client
	/// </summary>
	protected RequestBuilder Builder = new();

	/// <summary>
	/// Token used to cancel operations
	/// </summary>
	protected CancellationToken CancellationToken = CancellationToken.None;

	/// <summary>
	/// Sets the <see cref="HttpClient"/> instance of this client
	/// </summary>
	public void SetClient(HttpClient client)
	{
		_client = client;

		_client.DefaultRequestHeaders.UserAgent.Add(
			new ProductInfoHeaderValue(Metadata.GUID, Metadata.VERSION)
		);
		_client.Timeout = TimeSpan.FromMinutes(5);
	}

	/// <summary>
	/// Sets the <see cref="CancellationToken"/> of this client
	/// </summary>
	public void SetCancellationToken(CancellationToken ct)
	{
		CancellationToken = ct;
	}

	/// <summary>
	/// Sets the <see cref="RequestBuilder"/> of this client
	/// </summary>
	public void SetBuilder(RequestBuilder builder)
	{
		Builder = new RequestBuilder(builder);
	}

	#endregion

	protected ThunderstoreClient()
	{
		SetClient(new HttpClient());
	}

	#region Requests

	/// <summary>
	/// Sends the given request
	/// </summary>
	protected async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
	{
		if (_client == null)
			throw new NullReferenceException($"Expected '{nameof(_client)}' to be initialized");

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
		_client?.Dispose();
	}
}
