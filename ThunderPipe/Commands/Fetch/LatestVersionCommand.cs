using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace ThunderPipe.Commands.Fetch;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class LatestVersionCommand : AsyncCommand<Settings.Fetch.BaseSettings>
{
	private readonly ILogger<LatestVersionCommand> _logger;

	public LatestVersionCommand(ILogger<LatestVersionCommand> logger)
	{
		_logger = logger;
	}

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
