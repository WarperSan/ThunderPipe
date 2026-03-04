using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
internal sealed class PackageApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public PackageApiClient(RequestBuilder builder, HttpClient client, CancellationToken ct)
		: base(builder, client, ct) { }

	/// <summary>
	/// Gets the latest version of the package by the given team by the given name
	/// </summary>
	public async Task<string> GetVersion(string team, string name)
	{
		var package = await GetPackage(team, name);

		return package.LatestPackage.Version;
	}

	/// <summary>
	/// Gets the package by the given team by the given name
	/// </summary>
	/// <param name="team"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	private Task<Models.API.GetPackage.Response> GetPackage(string team, string name)
	{
		var request = new RequestBuilder(Builder)
			.Get()
			.ToEndpoint($"api/experimental/package/{team}/{name}/")
			.Build();

		return SendRequest<Models.API.GetPackage.Response>(request);
	}
}
