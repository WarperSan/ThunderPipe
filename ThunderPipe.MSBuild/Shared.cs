using System.Net.Http.Headers;

namespace ThunderPipe.MSBuild.Tasks;

internal static class Shared
{
	/// <summary>
	/// Shared instance of <see cref="HttpClient"/>
	/// </summary>
	public static readonly Lazy<HttpClient> Client = new(() =>
	{
		var client = new HttpClient();

		client.DefaultRequestHeaders.UserAgent.Add(
			new ProductInfoHeaderValue(nameof(MSBuild), Metadata.VERSION)
		);

		return client;
	});
}
