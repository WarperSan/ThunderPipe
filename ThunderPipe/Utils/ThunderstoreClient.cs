using System.Net.Http.Headers;
using Newtonsoft.Json;
using Spectre.Console;

namespace ThunderPipe.Utils;

/// <summary>
/// Class that handles HTTP requests to Thunderstore
/// </summary>
internal sealed class ThunderstoreClient : HttpClient
{
	private const string API_EXPERIMENTAL = "api/experimental/";
	public const string API_VALIDATE_ICON = API_EXPERIMENTAL + "submission/validate/icon/";

	public const string API_VALIDATE_MANIFEST =
		API_EXPERIMENTAL + "submission/validate/manifest-v1/";

	public const string API_VALIDATE_README = API_EXPERIMENTAL + "submission/validate/readme/";
	public const string API_INITIATE_UPLOAD = API_EXPERIMENTAL + "usermedia/initiate-upload/";
	public const string API_ABORT_UPLOAD = API_EXPERIMENTAL + "usermedia/{UUID}/abort-upload/";
	public const string API_FINISH_UPLOAD = API_EXPERIMENTAL + "usermedia/{UUID}/finish-upload/";
	public const string API_SUBMIT_PACKAGE = API_EXPERIMENTAL + "submission/submit/";
	public const string API_COMMUNITY_PAGE = API_EXPERIMENTAL + "community/";
	public const string API_CATEGORIES_PAGE = API_EXPERIMENTAL + "community/{COMMUNITY}/category/";
	public const string API_DEPENDENCY_VERSION =
		API_EXPERIMENTAL + "package/{NAMESPACE}/{NAME}/{VERSION}/";

	private ThunderstoreClient()
	{
		DefaultRequestHeaders.UserAgent.Add(
			new ProductInfoHeaderValue(Metadata.GUID, Metadata.VERSION)
		);
		Timeout = TimeSpan.FromMinutes(5);
	}

	/// <summary>
	/// Sends the given request
	/// </summary>
	public static async Task<HttpResponseMessage> SendRequest(
		HttpRequestMessage request,
		CancellationToken cancellationToken
	)
	{
		using var client = new ThunderstoreClient();

		return await client.SendAsync(request, cancellationToken);
		;
	}

	/// <summary>
	/// Sends the given request, and returns the JSON response
	/// </summary>
	public static async Task<T?> SendRequest<T>(
		HttpRequestMessage request,
		CancellationToken cancellationToken
	)
	{
		var response = await SendRequest(request, cancellationToken);
		var content = await response.Content.ReadAsStringAsync(cancellationToken);

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
