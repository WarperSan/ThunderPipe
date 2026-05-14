using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.MSBuild.Tasks.Factories;

/// <summary>
/// Class to create instances of <see cref="HttpApiClient"/>
/// </summary>
internal static class HttpApiClientFactory
{
	/// <summary>
	/// Creates a new instance of <see cref="HttpApiClient"/>
	/// </summary>
	public static HttpApiClient Create(ILogger? logger = null)
	{
		var httpClient = new HttpClient();

		httpClient.DefaultRequestHeaders.UserAgent.Add(
			new ProductInfoHeaderValue(ThisAssembly.Constants.Name, ThisAssembly.Constants.Version)
		);

		return new HttpApiClient(httpClient, logger);
	}
}
