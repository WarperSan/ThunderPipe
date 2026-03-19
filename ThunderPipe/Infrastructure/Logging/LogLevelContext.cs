using Microsoft.Extensions.Logging;

namespace ThunderPipe.Infrastructure.Logging;

/// <summary>
/// Holds the current <see cref="LogLevel"/>
/// </summary>
internal sealed class LogLevelContext
{
	/// <summary>
	/// Current minimum log level
	/// </summary>
	public LogLevel Level { get; set; }
}
