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
		var color = GetLogLevelConsoleColor(logEntry.LogLevel);
		var level = GetLogLevelString(logEntry.LogLevel);

		textWriter.WriteLine($"[{color}{level}\x1b[0m] {message}");
	}

	private static string GetLogLevelString(LogLevel logLevel) =>
		logLevel switch
		{
			LogLevel.Trace => "Trace",
			LogLevel.Debug => "Debug",
			LogLevel.Information => "Info",
			LogLevel.Warning => "Warning",
			LogLevel.Error => "Error",
			LogLevel.Critical => "Critical",
			_ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
		};

	private static string GetLogLevelConsoleColor(LogLevel logLevel) =>
		logLevel switch
		{
			LogLevel.Trace => "\x1b[90m",
			LogLevel.Debug => "\x1b[37m",
			LogLevel.Information => "\x1b[32m",
			LogLevel.Warning => "\x1b[1;33m",
			LogLevel.Error => "\x1b[31m",
			LogLevel.Critical => "\x1b[1;97;41m",
			_ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
		};
}
