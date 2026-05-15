using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to categories
/// </summary>
public sealed class PackageApiClient
{
	private readonly HttpApiClient _client;
	private readonly RequestBuilder _builder;
	private readonly ILogger? _logger;

	public PackageApiClient(HttpApiClient client, RequestBuilder builder, ILogger? logger = null)
	{
		_client = client;
		_builder = builder;
		_logger = logger;
	}

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
		var request = new RequestBuilder(_builder)
			.Get()
			.ToEndpoint($"api/experimental/package/{team}/{name}/")
			.Build();

		var response = await _client.SendRequest<Models.Web.GetPackage.Response>(request, ct);

		response.LogErrors(_logger);
		response.EnsureSuccess(out var data);

		return data;
	}
}
