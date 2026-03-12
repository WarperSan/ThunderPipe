using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Fetch;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class LatestVersionCommand : AsyncCommand<Settings.Fetch.LatestVersionSettings>
{
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

		var version = await client.GetVersion(settings.Team, settings.Name);
		Console.WriteLine(version);

		return 0;
	}
}
