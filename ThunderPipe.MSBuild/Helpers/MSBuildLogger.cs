using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ThunderPipe.MSBuild.Tasks.Helpers;

// ReSharper disable once InconsistentNaming
/// <summary>
/// Class that adds <see cref="ILogger"/> to a <see cref="TaskLoggingHelper"/> instance
/// </summary>
public class MSBuildLogger : ILogger
{
	private readonly TaskLoggingHelper _logger;

	public MSBuildLogger(TaskLoggingHelper logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter
	)
	{
		var message = formatter.Invoke(state, exception);

		switch (logLevel)
		{
			case LogLevel.Trace:
			case LogLevel.Debug:
				_logger.LogMessage(MessageImportance.Low, message);
				break;
			case LogLevel.Information:
				_logger.LogMessage(MessageImportance.High, message);
				break;
			case LogLevel.Warning:
				_logger.LogWarning(message);
				break;
			case LogLevel.Error:
			case LogLevel.Critical:
				_logger.LogError(message);
				break;
			case LogLevel.None:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
		}
	}

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel) => true;

	/// <inheritdoc />
	public IDisposable? BeginScope<TState>(TState state)
		where TState : notnull => null;
}
