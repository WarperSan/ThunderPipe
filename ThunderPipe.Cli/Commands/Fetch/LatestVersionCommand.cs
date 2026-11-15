using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Cli.Commands.Fetch;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class LatestVersionCommand : AsyncCommand<Settings.Fetch.LatestVersionSettings>
{
	private readonly HttpClient _client;
	private readonly ILogger _logger;

	public LatestVersionCommand(HttpClient client, ILogger logger)
	{
		_client = client;
		_logger = logger;
	}

	/// <inheritdoc />
	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		Settings.Fetch.LatestVersionSettings settings,
		CancellationToken cancellationToken
	)
	{
		var builder = new RequestBuilder().ToUri(settings.Host!);
		var client = new PackageApiClient(_client, builder, _logger);

		var version = await client.GetVersion(settings.Team, settings.Name, cancellationToken);

		if (version == null)
		{
			_logger.LogError(
				"Could not find the version of a package from '{Team}' named '{Name}'.",
				settings.Team,
				settings.Name
			);

			return 1;
		}

		Console.WriteLine(version);

		return 0;
	}
}
