using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by any command
/// </summary>
public abstract class BaseCommandSettings : CommandSettings
{
	[CommandOption("--log-level")]
	[Description("Minimum logging level")]
#if DEBUG
	[DefaultValue(LogLevel.Debug)]
#else
	[DefaultValue(LogLevel.Information)]
#endif
	public LogLevel LogLevel { get; init; }
}
