using Spectre.Console;

namespace ThunderPipe.Utils;

/// <summary>
/// Class holding useful methods for <see cref="AnsiConsole"/>
/// </summary>
internal static class Log
{
	/// <summary>
	/// Writes the given message to the console
	/// </summary>
	public static void WriteLine(string message)
	{
		AnsiConsole.Markup(message);
		AnsiConsole.WriteLine();
	}
	
	/// <summary>
	/// Formats a byte size into readable text
	/// </summary>
	public static string GetSizeString(long byteSize)
	{
		double finalSize = byteSize;
		string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
		var suffixIndex = 0;
		
		while (finalSize >= 1024 && suffixIndex < suffixes.Length)
		{
			finalSize /= 1024;
			suffixIndex++;
		}
		
		return $"{finalSize:F2} {suffixes[suffixIndex]}";
	}
}