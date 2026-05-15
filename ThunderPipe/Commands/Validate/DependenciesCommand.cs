using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;
using ThunderPipe.Settings.Validate;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DependenciesCommand : AsyncCommand<DependenciesSettings>
{
	private readonly HttpApiClient _apiClient;
	private readonly ILogger _logger;

	public DependenciesCommand(HttpApiClient apiClient, ILogger logger)
	{
		_apiClient = apiClient;
		_logger = logger;
	}

	/// <inheritdoc />
	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		DependenciesSettings settings,
		CancellationToken cancellationToken
	)
	{
		var builder = new RequestBuilder().ToUri(settings.Host!);
		var client = new DependencyApiClient(_apiClient, builder, _logger);

		var missingDependencies = await client.GetMissing(settings.Dependencies, cancellationToken);

		if (missingDependencies.Count > 0)
		{
			var listString = "- " + string.Join("\n- ", missingDependencies);
			throw new KeyNotFoundException(
				$"Could not find a package for the following dependency strings:\n{listString}"
			);
		}

		_logger.LogInformation("All dependencies have been found!");
		return 0;
	}
}
