using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Fetch;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class LatestVersionCommand : BaseCommand<Settings.Fetch.LatestVersionSettings>
{
	/// <inheritdoc />
	public LatestVersionCommand(ILogger logger)
		: base(logger) { }

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
		client.CancellationToken = cancellationToken;
		client.Logger = Logger;

		var version = await client.GetVersion(settings.Team, settings.Name);
		Console.WriteLine(version);

		return 0;
	}
}
