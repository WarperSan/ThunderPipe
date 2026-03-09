using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace ThunderPipe.Infrastructure;

/// <summary>
/// Console formatter used to create simplistic logs
/// </summary>
internal sealed class MinimalConsoleFormatter : ConsoleFormatter
{
	public MinimalConsoleFormatter()
		: base(nameof(MinimalConsoleFormatter)) { }

	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <inheritdoc/>
	public override void Write<TState>(
		in LogEntry<TState> logEntry,
		IExternalScopeProvider? scopeProvider,
		TextWriter textWriter
	)
	{
		var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

		if (logEntry.LogLevel != LogLevel.None)
		{
			textWriter.Write("[");
			textWriter.Write(GetLogLevelConsoleColor(logEntry.LogLevel));
			textWriter.Write(GetLogLevelString(logEntry.LogLevel));
			textWriter.Write("\x1b[39m\x1b[22m] ");
		}

		textWriter.WriteLine(message);
	}

	private static string GetLogLevelString(LogLevel logLevel) =>
		logLevel switch
		{
			LogLevel.Trace => "trce",
			LogLevel.Debug => "dbug",
			LogLevel.Information => "info",
			LogLevel.Warning => "warn",
			LogLevel.Error => "fail",
			LogLevel.Critical => "crit",
			_ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
		};

	private static string GetLogLevelConsoleColor(LogLevel logLevel) =>
		logLevel switch
		{
			LogLevel.Trace => "\x1b[37m",
			LogLevel.Debug => "\x1b[37m",
			LogLevel.Information => "\x1b[32m",
			LogLevel.Warning => "\x1b[1m\x1b[33m",
			LogLevel.Error => "\x1b[30m",
			LogLevel.Critical => "\x1b[1m\x1b[37m",
			_ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
		};
}
