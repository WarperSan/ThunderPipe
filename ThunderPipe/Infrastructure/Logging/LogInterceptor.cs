using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;

namespace ThunderPipe.Infrastructure.Logging;

internal sealed class LogInterceptor : ICommandInterceptor
{
	#if DEBUG
	public const LogLevel MINIMUM_LEVEL = LogLevel.Debug;
	#else
	public const LogLevel MINIMUM_LEVEL = LogLevel.Information;
	#endif

	private readonly LogLevelContext _context;

	public LogInterceptor(LogLevelContext context)
	{
		_context = context;
	}

	/// <inheritdoc/>
	public void Intercept(CommandContext context, CommandSettings settings)
	{
		if (settings is not BaseCommandSettings baseSettings)
			return;

		_context.Level = baseSettings.LogLevel;
	}
}
