using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace ThunderPipe.Utils;

/// <summary>
/// Class allowing to build <see cref="HttpRequestMessage"/>
/// </summary>
internal sealed class RequestBuilder
{
	private readonly UriBuilder _uriBuilder = new();
	private HttpContent? _content;
	private AuthenticationHeaderValue? _authHeader;

	public RequestBuilder(string host)
	{
		_uriBuilder.Host = host;
	}

	/// <summary>
	/// Sets the endpoint of this request
	/// </summary>
	public RequestBuilder ToEndpoint(string endpoint)
	{
		_uriBuilder.Path = endpoint;
		return this;
	}

	/// <summary>
	/// Sets the payload of this request
	/// </summary>
	public RequestBuilder WithJson(object json)
	{
		var serializedJson = JsonConvert.SerializeObject(json);
		
		_content = new StringContent(
			serializedJson,
			Encoding.UTF8,
			MediaTypeNames.Application.Json
		);

		return this;
	}

	/// <summary>
	/// Sets the authentication token
	/// </summary>
	public RequestBuilder WithAuth(string token)
	{
		_authHeader = new AuthenticationHeaderValue(
			"Bearer",
			token
		);

		return this;
	}

	/// <summary>
	/// Builds the <see cref="HttpRequestMessage"/> from this request
	/// </summary>
	public HttpRequestMessage Build()
	{
		var request = new HttpRequestMessage();

		request.Method = HttpMethod.Post;
		request.RequestUri = _uriBuilder.Uri;
		request.Content = _content;
		request.Headers.Authorization = _authHeader;

		return request;
	}
}