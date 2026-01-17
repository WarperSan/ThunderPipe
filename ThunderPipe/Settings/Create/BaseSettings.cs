using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings.Create;

/// <summary>
/// Settings used by any creation command
/// </summary>
public abstract class BaseSettings : CommandSettings
{
	[CommandOption("--directory")]
	[Description("Directory from which the relative paths begin")]
	[DefaultValue(null)]
	public string? Directory { get; set; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrEmpty(Directory))
			Directory = System.IO.Directory.GetCurrentDirectory();

		if (!System.IO.Directory.Exists(Directory))
			return ValidationResult.Error("Directory does not exist.");

		return base.Validate();
	}
}
