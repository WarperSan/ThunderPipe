using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Infrastructure;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by any command
/// </summary>
internal abstract class BaseCommandSettings : CommandSettings
{
	[CommandOption("--log-level")]
	[Description("Minimum level of messages to display")]
	[DefaultValue(LogInterceptor.MINIMUM_LEVEL)]
	public LogLevel LogLevel { get; init; }
}
