using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
public sealed class PackageApiClient : ThunderstoreClient
{
	/// <summary>
	/// Gets the latest version of the package by the given team by the given name
	/// </summary>
	public async Task<PackageVersion> GetVersion(
		Team team,
		PackageName name,
		CancellationToken ct = default
	)
	{
		var package = await GetPackage(team, name, ct);

		return package.LatestPackage.Version;
	}

	/// <summary>
	/// Gets the package by the given team by the given name
	/// </summary>
	private async Task<Models.Web.GetPackage.Response> GetPackage(
		Team team,
		PackageName name,
		CancellationToken ct
	)
	{
		var request = new RequestBuilder(Builder)
			.Get()
			.ToEndpoint($"api/experimental/package/{team}/{name}/")
			.Build();

		var response = await SendRequest<Models.Web.GetPackage.Response>(request, ct);

		response.LogErrors(Logger);
		response.EnsureSuccess(out var data);

		return data;
	}
}
