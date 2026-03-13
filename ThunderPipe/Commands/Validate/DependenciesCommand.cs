using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;
using ThunderPipe.Settings.Validate;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DependenciesCommand : BaseCommand<DependenciesSettings>
{
	/// <inheritdoc />
	public DependenciesCommand(ILogger logger)
		: base(logger) { }

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		DependenciesSettings settings,
		CancellationToken cancellationToken
	)
	{
		var builder = new RequestBuilder().ToUri(settings.Host!);
		using var client = new DependencyApiClient();
		client.Builder = builder;
		client.CancellationToken = cancellationToken;
		client.Logger = Logger;

		var missingDependencies = await client.GetMissing(settings.Dependencies);

		if (missingDependencies.Count > 0)
		{
			var listString = "- " + string.Join("\n- ", missingDependencies);
			throw new KeyNotFoundException(
				$"Could not find a package for the following dependency strings:\n{listString}"
			);
		}

		Logger.LogInformation("All dependencies have been found!");
		return 0;
	}
}
