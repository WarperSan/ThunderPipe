using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Settings.Validate;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DependenciesCommand : AsyncCommand<DependenciesSettings>
{
	private readonly ILogger<DependenciesCommand> _logger;

	public DependenciesCommand(ILogger<DependenciesCommand> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		DependenciesSettings settings,
		CancellationToken cancellationToken
	)
	{
		var dependencyStrings = settings.Dependencies;
		var builder = new RequestBuilder().ToUri(settings.Repository!);
		using var client = new DependencyApiClient(builder, cancellationToken);

		var missingDependencies = await client.GetMissing(dependencyStrings);

		if (missingDependencies.Count > 0)
		{
			var listString = "- " + string.Join("\n- ", missingDependencies);

			_logger.LogError("Failed to find these dependencies:\n{Dependencies}", listString);
			return 1;
		}

		_logger.LogInformation("All dependencies have been found!");
		return 0;
	}
}
