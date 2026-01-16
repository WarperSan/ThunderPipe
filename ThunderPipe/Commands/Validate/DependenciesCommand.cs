using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DependenciesCommand : AsyncCommand<ValidateDependenciesSettings>
{
	private readonly ILogger<DependenciesCommand> _logger;

	public DependenciesCommand(ILogger<DependenciesCommand> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		ValidateDependenciesSettings settings,
		CancellationToken cancellationToken
	)
	{
		var dependencyStrings = settings.Dependencies!;
		var builder = new RequestBuilder().ToUri(settings.Repository!);

		var dependencies = await ThunderstoreAPI.FindDependencies(
			dependencyStrings,
			builder,
			cancellationToken
		);

		var missingDependencies = dependencyStrings.Where(c => !dependencies.ContainsKey(c));

		if (missingDependencies.Any())
		{
			var listString = "- " + string.Join("\n- ", missingDependencies);

			_logger.LogError("Failed to find these dependencies:\n{Dependencies}", listString);
			return 1;
		}

		_logger.LogInformation("All dependencies have been found!");
		return 0;
	}
}
