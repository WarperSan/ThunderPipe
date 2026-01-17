using System.Net.Http.Headers;
using Newtonsoft.Json;
using Spectre.Console;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
internal abstract class ThunderstoreClient : HttpClient
{
	/// <summary>
	/// Default <see cref="RequestBuilder"/> for this client
	/// </summary>
	protected readonly RequestBuilder Builder;

	/// <summary>
	/// Token used to cancel operations
	/// </summary>
	protected readonly CancellationToken CancellationToken;

	protected ThunderstoreClient(RequestBuilder builder, CancellationToken ct)
	{
		Builder = builder.Copy();
		CancellationToken = ct;

		DefaultRequestHeaders.UserAgent.Add(
			new ProductInfoHeaderValue(Metadata.GUID, Metadata.VERSION)
		);
		Timeout = TimeSpan.FromMinutes(5);
	}

	/// <summary>
	/// Sends the given request
	/// </summary>
	protected async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
	{
		return await SendAsync(request, CancellationToken);
	}

	/// <summary>
	/// Sends the given request, and returns the JSON response
	/// </summary>
	protected async Task<T?> SendRequest<T>(HttpRequestMessage request)
	{
		var response = await SendRequest(request);
		var content = await response.Content.ReadAsStringAsync(CancellationToken);

		try
		{
			return JsonConvert.DeserializeObject<T>(content);
		}
		catch (JsonSerializationException e)
		{
			AnsiConsole.MarkupLine(
				$"Failed to deserialize response:\n{e.Message.EscapeMarkup()}\n\n[gray]{content.EscapeMarkup()}[/]"
			);
			return default;
		}
		catch (JsonReaderException e)
		{
			AnsiConsole.MarkupLine(
				$"Failed to read response:\n{e.Message.EscapeMarkup()}\n\n[gray]{content.EscapeMarkup()}[/]"
			);
			return default;
		}
	}
}
