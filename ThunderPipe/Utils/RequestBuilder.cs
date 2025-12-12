using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace ThunderPipe.Utils;

/// <summary>
/// Class allowing to build <see cref="HttpRequestMessage"/>
/// </summary>
/// <remarks>
/// Based off <a href="https://docs.oracle.com/en/java/javase/11/docs/api/java.net.http/java/net/http/HttpRequest.Builder.html">this</a></remarks>
internal sealed class RequestBuilder
{
	private UriBuilder _uriBuilder = new();
	private HttpContent? _content;
	private AuthenticationHeaderValue? _authHeader;

	/// <summary>
	/// Sets the host of this request
	/// </summary>
	public RequestBuilder ToHost(string host)
	{
		_uriBuilder.Host = host;
		return this;
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
	/// Sets the JSON payload of this request
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
	public RequestBuilder WithAuth(string? token)
	{
		_authHeader = new AuthenticationHeaderValue(
			"Bearer",
			token
		);

		return this;
	}

	/// <summary>
	/// Copies this builder to a brand-new builder with the same state
	/// </summary>
	public RequestBuilder Copy()
	{
		var newBuilder = new RequestBuilder
		{
			_uriBuilder = new UriBuilder(_uriBuilder.Uri)
		};

		if (_authHeader != null)
		{
			newBuilder._authHeader = new AuthenticationHeaderValue(
				_authHeader.Scheme,
				_authHeader.Parameter
			);
		}

		if (_content != null)
			newBuilder._content = new StreamContent(_content.ReadAsStream());
		
		return newBuilder;
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