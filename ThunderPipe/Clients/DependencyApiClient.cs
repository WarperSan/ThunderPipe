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
	/// Finds the missing dependencies
	/// </summary>
	public async Task<ISet<string>> GetMissing(string[] dependencies)
	{
		var tempBuilder = Builder
			.Copy()
			.Get()
			.ToEndpoint("api/experimental/package/{NAMESPACE}/{NAME}/{VERSION}/");

		var dependenciesToFind = new HashSet<string>(dependencies);

		foreach (var dependency in dependencies)
		{
			RegexHelper.SplitDependency(
				dependency,
				out var @namespace,
				out var name,
				out var version
			);

			if (@namespace == null || name == null || version == null)
				continue;

			var request = tempBuilder
				.Copy()
				.SetPathParameter("NAMESPACE", @namespace)
				.SetPathParameter("NAME", name)
				.SetPathParameter("VERSION", version)
				.Build();

			var response = await SendRequest<Models.API.GetDependency.Response>(request);

			if (response == null)
				continue;

			if (!response.IsActive)
				continue;

			dependenciesToFind.Remove(dependency);
		}

		return dependenciesToFind;
	}
}
