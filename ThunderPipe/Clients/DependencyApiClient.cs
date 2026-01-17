using System.Text.RegularExpressions;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call API endpoints related to dependencies
/// </summary>
internal sealed class DependencyApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public DependencyApiClient(RequestBuilder builder, CancellationToken ct)
		: base(builder, ct) { }

	/// <summary>
	/// Finds the dependencies
	/// </summary>
	public async Task<Dictionary<string, Models.API.GetDependency.Response>> FindDependencies(
		string[] dependencies
	)
	{
		var tempBuilder = Builder
			.Copy()
			.Get()
			.ToEndpoint("api/experimental/package/{NAMESPACE}/{NAME}/{VERSION}/");

		var foundDependencies = new Dictionary<string, Models.API.GetDependency.Response>();

#pragma warning disable SYSLIB1045
		var regex = new Regex(
			$"^(?<namespace>{ThunderstoreClient.REGEX_NAMESPACE})-(?<name>{ThunderstoreClient.REGEX_NAME})-(?<version>{ThunderstoreClient.REGEX_VERSION})$"
		);
#pragma warning restore SYSLIB1045

		foreach (var dependency in dependencies)
		{
			var match = regex.Match(dependency);

			string? @namespace = null;
			string? name = null;
			string? version = null;

			if (match.Groups.TryGetValue("namespace", out var namespaceGroup))
				@namespace = namespaceGroup.Value;

			if (match.Groups.TryGetValue("name", out var nameGroup))
				name = nameGroup.Value;

			if (match.Groups.TryGetValue("version", out var versionGroup))
				version = versionGroup.Value;

			if (@namespace == null || name == null || version == null)
				continue;

			var request = tempBuilder
				.Copy()
				.SetPathParameter("NAMESPACE", @namespace)
				.SetPathParameter("NAME", name)
				.SetPathParameter("VERSION", version)
				.Build();

			var response = await SendRequest<Models.API.GetDependency.Response>(request);

			if (!response.IsActive)
				continue;

			foundDependencies[dependency] = response;
		}

		return foundDependencies;
	}
}
