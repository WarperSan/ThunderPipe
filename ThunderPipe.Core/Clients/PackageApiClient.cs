using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
internal sealed class PackageApiClient : ThunderstoreClient
{
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
	private Task<Models.Web.GetPackage.Response> GetPackage(string team, string name)
	{
		var request = new RequestBuilder(Builder)
			.Get()
			.ToEndpoint($"api/experimental/package/{team}/{name}/")
			.Build();

		return SendRequest<Models.Web.GetPackage.Response>(request);
	}
}
