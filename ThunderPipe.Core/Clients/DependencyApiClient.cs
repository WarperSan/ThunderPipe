using ThunderPipe.Core.Models.API;
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
	public async Task<ISet<PackageDependency>> GetMissing(PackageDependency[] dependencies)
	{
		const string NAMESPACE = "NAMESPACE";
		const string NAME = "NAME";
		const string VERSION = "VERSION";

		var tempBuilder = new RequestBuilder(Builder)
			.Get()
			.ToEndpoint($"api/experimental/package/{{{NAMESPACE}}}/{{{NAME}}}/{{{VERSION}}}/");

		var dependenciesFound = new HashSet<PackageDependency>();

		foreach (var dependency in dependencies)
		{
			if (!dependency.IsValid())
				continue;

			var request = new RequestBuilder(tempBuilder)
				.SetPathParameter(NAMESPACE, dependency.Team!)
				.SetPathParameter(NAME, dependency.Name!)
				.SetPathParameter(VERSION, dependency.Version!)
				.Build();

			// TODO: Check if endpoint has changed
			var response = await SendRequest<Models.Web.GetDependency.Response>(request);

			if (!response.IsActive)
				continue;

			dependenciesFound.Add(dependency);
		}

		return dependencies.Except(dependenciesFound).ToHashSet();
	}
}
