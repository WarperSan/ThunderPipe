using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;
using Xunit;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure;

/// <summary>
/// Only loopback/.localhost hosts are permitted. No mocked responses are used.
/// Supplemental fixture values are passed using environment variables.
/// </summary>
public sealed class ThunderstoreFixture : IAsyncLifetime
{
	private readonly HttpClient _http;
	private readonly string _traceFile;
	private static readonly object TraceLock = new();

	public ThunderstoreFixture()
	{
		BaseUri = Settings.BaseUri;
		RequireLocal(BaseUri);
		if (BaseUri.Scheme != Uri.UriSchemeHttp)
			throw new InvalidOperationException(
				"Integration tests require the local HTTP Docker endpoint."
			);

		_traceFile =
			Environment.GetEnvironmentVariable("THUNDERPIPE_HTTP_TRACE")
			?? Path.GetFullPath(
				Path.Combine(
					AppContext.BaseDirectory,
					"..",
					"..",
					"..",
					"artifacts",
					"thunderpipe-http.log"
				)
			);
		Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_traceFile))!);
		var handler = new LocalOnlyHandler(
			new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false },
			Record
		);
		_http = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(8) };
		Builder = new RequestBuilder().ToUri(BaseUri);
	}

	public FixtureData Data => Settings.Supplemental;
	public Uri BaseUri { get; }
	public RequestBuilder Builder { get; }
	public HttpClient Http => _http;
	public CommunityApiClient Communities => new(_http, Builder);
	public CategoryApiClient Categories => new(_http, Builder);
	public DependencyApiClient Dependencies => new(_http, Builder);
	public PackageApiClient Packages => new(_http, Builder);
	public ValidationApiClient Validation => new(_http, Builder);
	public PublishApiClient Publish => new(_http, Builder);

	public static void RequireLocal(Uri uri)
	{
		if (
			uri.Scheme is not ("http" or "https")
			|| !(
				uri.IsLoopback
				|| uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
			)
		)
			throw new InvalidOperationException($"Refusing request to non-local host: {uri.Host}");
	}

	public void Evidence(string message) => Record($"EVIDENCE {DateTimeOffset.UtcNow:O} {message}");

	private void Record(string message)
	{
		lock (TraceLock)
			File.AppendAllText(_traceFile, message + Environment.NewLine);
	}

	public ValueTask InitializeAsync() => ValueTask.CompletedTask;

	public ValueTask DisposeAsync()
	{
		_http.Dispose();
		return ValueTask.CompletedTask;
	}

}

internal sealed class LocalOnlyHandler : DelegatingHandler
{
	private readonly Action<string> _trace;

	public LocalOnlyHandler(HttpMessageHandler inner, Action<string> trace)
		: base(inner)
	{
		_trace = trace;
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken ct
	)
	{
		if (request.RequestUri is null)
			throw new InvalidOperationException("Request has no URI.");
		ThunderstoreFixture.RequireLocal(request.RequestUri);
		var started = DateTimeOffset.UtcNow;
		try
		{
			var response = await base.SendAsync(request, ct);
			_trace(
				$"{started:O} {request.Method} {request.RequestUri.GetLeftPart(UriPartial.Path)} -> {(int)response.StatusCode}"
			);
			return response;
		}
		catch (Exception e)
		{
			_trace(
				$"{started:O} {request.Method} {request.RequestUri.GetLeftPart(UriPartial.Path)} -> {e.GetType().Name}: {e.Message}"
			);
			throw;
		}
	}
}
