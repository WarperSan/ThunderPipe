using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Infrastructure.TypeConverters;

namespace ThunderPipe.Settings.Validate;

/// <summary>
/// Settings used by <see cref="Commands.Validate.PackageCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class PackageSettings : BaseValidateSettings
{
	[CommandArgument(0, "<package-folder>")]
	[Description("Path to the folder containing the package files")]
	[TypeConverter(typeof(PathTypeConverter))]
	public required string PackageFolder { get; init; }

	[CommandArgument(1, "<team>")]
	[Description("Team that will publish the package")]
	public required Team Team { get; init; }

	[CommandOption("--icon-path")]
	[Description("Relative path to the package icon")]
	[DefaultValue("./icon.png")]
	public string? IconPath { get; set; }

	[CommandOption("--manifest-path")]
	[Description("Relative path to the manifest file")]
	[DefaultValue("./manifest.json")]
	public string? ManifestPath { get; set; }

	[CommandOption("--readme-path")]
	[Description("Relative path to the README file")]
	[DefaultValue("./README.md")]
	public string? ReadmePath { get; set; }

	[CommandOption("--token", true)]
	[Description("Service account API token for authentication")]
	public required string Token { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Directory.Exists(PackageFolder))
			return ValidationResult.Error($"No folder was found at '{PackageFolder}'.");

		if (!Team.IsValid())
			return ValidationResult.Error($"'{Team}' is not a valid package team.");

		IconPath = Path.GetFullPath(IconPath!, PackageFolder);

		if (!File.Exists(IconPath))
			return ValidationResult.Error($"No file was found at '{IconPath}'.");

		IconPath = Path.GetFullPath(IconPath!, PackageFolder);

		if (!File.Exists(IconPath))
			return ValidationResult.Error($"No file was found at '{IconPath}'.");

		ManifestPath = Path.GetFullPath(ManifestPath!, PackageFolder);

		if (!File.Exists(ManifestPath))
			return ValidationResult.Error($"No file was found at '{ManifestPath}'.");

		ReadmePath = Path.GetFullPath(ReadmePath!, PackageFolder);

		if (!File.Exists(ReadmePath))
			return ValidationResult.Error($"No file was found at '{ReadmePath}'.");

		if (string.IsNullOrWhiteSpace(Token))
			return ValidationResult.Error("Token cannot be empty.");

		return base.Validate();
	}
}
