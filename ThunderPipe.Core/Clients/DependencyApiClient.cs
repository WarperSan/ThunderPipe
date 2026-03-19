using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Models.Web.GetDependency;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call API endpoints related to dependencies
/// </summary>
public sealed class DependencyApiClient : ThunderstoreClient
{
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

		var tempBuilder = new RequestBuilder(Builder)
			.Get()
			.ToEndpoint($"api/experimental/package/{{{NAMESPACE}}}/{{{NAME}}}/{{{VERSION}}}/");

		var dependenciesFound = new ConcurrentBag<PackageDependency>();

		var tasks = dependencies.Select(async dependency =>
		{
			if (!dependency.IsValid())
				return;

			var request = new RequestBuilder(tempBuilder)
				.SetPathParameter(NAMESPACE, dependency.Team!)
				.SetPathParameter(NAME, dependency.Name!)
				.SetPathParameter(VERSION, dependency.Version!)
				.Build();

			var rawResponse = await SendRequest(request, ct);

			if (!rawResponse.IsSuccessStatusCode)
			{
				if (rawResponse.StatusCode != HttpStatusCode.NotFound)
					Logger?.LogDebug(
						"Expected '{ExpectedCode}', but got: {Code}",
						HttpStatusCode.NotFound,
						rawResponse.StatusCode
					);
				return;
			}

			var response = await ParseJson<Response>(rawResponse, ct);

			if (!response.IsActive)
				return;

			dependenciesFound.Add(dependency);
		});

		await Task.WhenAll(tasks);

		return dependencies.Except(dependenciesFound).ToArray();
	}
}
