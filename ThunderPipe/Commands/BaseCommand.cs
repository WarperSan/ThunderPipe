using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;

namespace ThunderPipe.Commands;

/// <summary>
/// Parent of any command
/// </summary>
internal abstract class BaseCommand<T> : AsyncCommand<T>
	where T : BaseCommandSettings
{
	protected readonly ILogger Logger;

	protected BaseCommand(ILogger logger)
	{
		Logger = logger;
	}
}
