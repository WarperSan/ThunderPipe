using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace ThunderPipe.Commands.Fetch;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class LatestVersionCommand : AsyncCommand<Settings.Fetch.BaseSettings>
{
	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		Settings.Fetch.BaseSettings baseSettings,
		CancellationToken cancellationToken
	)
	{
		return 0;
	}
}
