using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands.Publish;

namespace ThunderPipe.Settings.Publish;

/// <summary>
/// Settings used by <see cref="PackageCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PackageSettings : BaseSettings
{
	[CommandArgument(0, "<file>")]
	[Description("Path to the '.zip' file to publish")]
	public required string File { get; init; }

	[CommandArgument(1, "<team>")]
	[Description("Team that will own the published package")]
	public required string Team { get; init; }

	[CommandArgument(2, "<community>")]
	[Description("Slug of the community to publish the package to")]
	public required string Community { get; init; }

	[CommandOption("--category <CATEGORY>")]
	[Description("Category slug to label the package with")]
	public string[]? Categories { get; init; }

	[CommandOption("--has-nsfw|--nsfw")]
	[Description("Mark the package as containing NSFW content")]
	[DefaultValue(false)]
	public bool HasNsfw { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!System.IO.File.Exists(File))
			return ValidationResult.Error($"No file was found at '{File}'.");

		if (string.IsNullOrWhiteSpace(Team))
			return ValidationResult.Error("Team cannot be empty.");

		if (string.IsNullOrWhiteSpace(Community))
			return ValidationResult.Error("Community cannot be empty.");

		if (Categories != null && Categories.Any(string.IsNullOrEmpty))
			return ValidationResult.Error("Categories contains an empty item.");

		return base.Validate();
	}
}
