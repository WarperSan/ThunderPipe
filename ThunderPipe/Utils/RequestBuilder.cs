using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace ThunderPipe.Utils;

/// <summary>
/// Class allowing to build <see cref="HttpRequestMessage"/> with ease
/// </summary>
/// <remarks>
/// Based off <a href="https://docs.oracle.com/en/java/javase/11/docs/api/java.net.http/java/net/http/HttpRequest.Builder.html">this</a>
/// </remarks>
internal sealed class RequestBuilder
{
	#region Methods

	private HttpMethod _method = HttpMethod.Get;

	/// <summary>
	/// Sets the HTTP method
	/// </summary>
	private RequestBuilder WithMethod(HttpMethod method)
	{
		_method = method;
		return this;
	}

	/// <summary>
	/// Sets the HTTP method to <see cref="HttpMethod.Post"/>
	/// </summary>
	public RequestBuilder Post() => WithMethod(HttpMethod.Post);

	/// <summary>
	/// Sets the HTTP method to <see cref="HttpMethod.Put"/>
	/// </summary>
	public RequestBuilder Put() => WithMethod(HttpMethod.Put);

	#endregion

	#region URI

	private UriBuilder _uriBuilder = new();

	/// <summary>
	/// Sets the endpoint of this request
	/// </summary>
	public RequestBuilder ToEndpoint(string endpoint)
	{
		_uriBuilder.Path = endpoint;
		return this;
	}

	/// <summary>
	/// Sets the URL of this request
	/// </summary>
	public RequestBuilder ToUrl(string url)
	{
		_uriBuilder = new UriBuilder(url);
		return this;
	}

	/// <summary>
	/// Sets the URL of this request
	/// </summary>
	public RequestBuilder ToUri(Uri uri)
	{
		_uriBuilder = new UriBuilder(uri);
		return this;
	}

	#endregion

	#region Headers

	private AuthenticationHeaderValue? _authHeader;

	/// <summary>
	/// Sets the authentication token
	/// </summary>
	public RequestBuilder WithAuth(string? token)
	{
		_authHeader = new AuthenticationHeaderValue("Bearer", token);

		return this;
	}

	#endregion

	#region Content

	private HttpContent? _content;

	/// <summary>
	/// Sets the JSON payload of this request
	/// </summary>
	public RequestBuilder WithJSON(object json)
	{
		var serializedJson = JsonConvert.SerializeObject(json);

		var jsonContent = new StringContent(
			serializedJson,
			Encoding.UTF8,
			MediaTypeNames.Application.Json
		);

		return WithContent(jsonContent);
	}

	/// <summary>
	/// Sets the payload of this request
	/// </summary>
	public RequestBuilder WithContent(HttpContent content)
	{
		_content = content;
		return this;
	}

	#endregion

	/// <summary>
	/// Copies this builder to a brand-new builder with the same state
	/// </summary>
	public RequestBuilder Copy()
	{
		var newBuilder = new RequestBuilder
		{
			_uriBuilder = new UriBuilder(_uriBuilder.Uri),
			_method = _method,
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

		request.Method = _method;
		request.RequestUri = _uriBuilder.Uri;
		request.Content = _content;
		request.Headers.Authorization = _authHeader;

		return request;
	}
}
