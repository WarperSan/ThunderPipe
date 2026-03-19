using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Commands.Fetch;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class LatestVersionCommand : AsyncCommand<Settings.Fetch.LatestVersionSettings>
{
	private readonly ILogger _logger;

	public LatestVersionCommand(ILogger logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		Settings.Fetch.LatestVersionSettings settings,
		CancellationToken cancellationToken
	)
	{
		var builder = new RequestBuilder().ToUri(settings.Host!);
		using var client = new PackageApiClient();
		client.Builder = builder;
		client.Logger = _logger;

		var version = await client.GetVersion(settings.Team, settings.Name, cancellationToken);
		Console.WriteLine(version);

		return 0;
	}
}
