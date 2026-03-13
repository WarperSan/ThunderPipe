using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;

namespace ThunderPipe.Infrastructure;

internal sealed class LogInterceptor : ICommandInterceptor
{
#if DEBUG
	public const LogLevel MINIMUM_LEVEL = LogLevel.Debug;
#else
	public const LogLevel MINIMUM_LEVEL = LogLevel.Information;
#endif

	/// <summary>
	/// Current minimum log level
	/// </summary>
	public static LogLevel Level { get; private set; } = MINIMUM_LEVEL;

	/// <inheritdoc/>
	public void Intercept(CommandContext context, CommandSettings settings)
	{
		if (settings is not BaseCommandSettings baseSettings)
			return;

		Level = baseSettings.LogLevel;
	}
}
