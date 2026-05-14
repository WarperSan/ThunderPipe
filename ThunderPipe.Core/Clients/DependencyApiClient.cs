using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Models.Web.GetDependency;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to dependencies
/// </summary>
public sealed class DependencyApiClient
{
	private readonly HttpApiClient _client;
	private readonly RequestBuilder _builder;
	private readonly ILogger? _logger;

	public DependencyApiClient(HttpApiClient client, RequestBuilder builder, ILogger? logger = null)
	{
		_client = client;
		_builder = builder;
		_logger = logger;
	}

	/// <summary>
	/// Finds the missing dependencies
	/// </summary>
	public async Task<IReadOnlyCollection<PackageDependency>> GetMissing(
		IEnumerable<PackageDependency> dependencies,
		CancellationToken ct = default
	)
	{
		const string NAMESPACE = "NAMESPACE";
		const string NAME = "NAME";
		const string VERSION = "VERSION";

		var tempBuilder = new RequestBuilder(_builder)
			.Get()
			.ToEndpoint($"api/experimental/package/{{{NAMESPACE}}}/{{{NAME}}}/{{{VERSION}}}/");

		var dependenciesFound = new ConcurrentBag<PackageDependency>();

		var tasks = dependencies.Select(async dependency =>
		{
			if (!dependency.IsValid())
				return;

			var request = new RequestBuilder(tempBuilder)
				.SetPathParameter(NAMESPACE, dependency.Team)
				.SetPathParameter(NAME, dependency.Name)
				.SetPathParameter(VERSION, dependency.Version)
				.Build();

			var response = await _client.SendRequest<Response>(request, ct);

			response.LogErrors(_logger);

			if (!response.IsSuccess || response.Data == null)
				return;

			if (!response.Data.IsActive)
				return;

			dependenciesFound.Add(dependency);
		});

		await Task.WhenAll(tasks);

		return dependencies.Except(dependenciesFound).ToArray();
	}
}
