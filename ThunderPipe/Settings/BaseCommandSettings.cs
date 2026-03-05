using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by any command
/// </summary>
internal abstract class BaseCommandSettings : CommandSettings
{
	[CommandOption("--log-level")]
	[Description("Minimum level of messages to display")]
#if DEBUG
	[DefaultValue(LogLevel.Debug)]
#else
	[DefaultValue(LogLevel.Information)]
#endif
	public LogLevel LogLevel { get; init; }
}
