using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by <see cref="ValidatePackageCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ValidatePackageSettings : ValidateSettings
{
	[CommandArgument(0, "<package-folder>")]
	[Description("Folder containg the package's files")]
	public required string PackageFolder { get; init; }

	[CommandOption("--icon")]
	[Description("Path from the package folder to the icon file")]
	[DefaultValue("./icon.png")]
	public string? IconPath { get; init; }

	[CommandOption("--manifest")]
	[Description("Path from the package folder to the manifest file")]
	[DefaultValue("./manifest.json")]
	public string? ManifestPath { get; init; }

	[CommandOption("--readme")]
	[Description("Path from the package folder to the README file")]
	[DefaultValue("./README.md")]
	public string? ReadmePath { get; init; }

	[CommandOption("--team")]
	[Description("Team that will publish the package")]
	public string? Team { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Directory.Exists(PackageFolder))
			return ValidationResult.Error($"No folder was found at '{PackageFolder}'.");

		if (string.IsNullOrWhiteSpace(IconPath))
			return ValidationResult.Error("Icon path must be specified.");

		var iconPath = Path.GetFullPath(IconPath, PackageFolder);

		if (!File.Exists(iconPath))
			return ValidationResult.Error($"No file was found at '{iconPath}'.");

		if (string.IsNullOrWhiteSpace(ManifestPath))
			return ValidationResult.Error("Manifest path must be specified.");

		var manifestPath = Path.GetFullPath(ManifestPath, PackageFolder);

		if (!File.Exists(manifestPath))
			return ValidationResult.Error($"No file was found at '{manifestPath}'.");

		if (string.IsNullOrWhiteSpace(ReadmePath))
			return ValidationResult.Error("README path must be specified.");

		var readmePath = Path.GetFullPath(ReadmePath, PackageFolder);

		if (!File.Exists(readmePath))
			return ValidationResult.Error($"No file was found at '{readmePath}'.");

		if (string.IsNullOrWhiteSpace(Team))
			return ValidationResult.Error("Team must be specified.");

		return base.Validate();
	}
}
