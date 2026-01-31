using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;

namespace ThunderPipe.Infrastructure;

internal sealed class LogInterceptor : ICommandInterceptor
{
	/// <summary>
	/// Current minimum log level
	/// </summary>
	public static LogLevel Level { get; private set; } = LogLevel.None;

	/// <inheritdoc/>
	public void Intercept(CommandContext context, CommandSettings settings)
	{
		if (settings is not BaseCommandSettings baseSettings)
			return;

		Level = baseSettings.LogLevel;
	}
}
