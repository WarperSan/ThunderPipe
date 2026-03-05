using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Infrastructure.TypeConverters;

namespace ThunderPipe.Settings.Create;

/// <summary>
/// Settings used by any creation command
/// </summary>
internal abstract class BaseCreateSettings : BaseCommandSettings
{
	private const string OUTPUT_DIRECTORY_OPTION = "--output-dir";

	[CommandOption(OUTPUT_DIRECTORY_OPTION)]
	[Description("Directory used to resolve output file paths")]
	[DefaultValue(".")]
	[TypeConverter(typeof(PathTypeConverter))]
	public string? OutputDirectory { get; set; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(OutputDirectory))
			return ValidationResult.Error($"'{OUTPUT_DIRECTORY_OPTION}' cannot be empty.");

		OutputDirectory = Path.GetFullPath(OutputDirectory);

		if (!Directory.Exists(OutputDirectory))
			return ValidationResult.Error(
				$"'{OUTPUT_DIRECTORY_OPTION}' does not point to an existing directory."
			);

		return base.Validate();
	}
}
