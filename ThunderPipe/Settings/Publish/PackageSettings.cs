using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands.Publish;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Infrastructure.TypeConverters;

namespace ThunderPipe.Settings.Publish;

/// <summary>
/// Settings used by <see cref="PackageCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class PackageSettings : BasePublishSettings
{
	private const string CATEGORY_OPTION = "--category";

	[CommandArgument(0, "<file>")]
	[Description("Path to the '.zip' file to publish")]
	[TypeConverter(typeof(PathTypeConverter))]
	public required string File { get; init; }

	[CommandArgument(1, "<team>")]
	[Description("Team that will own the published package")]
	public required Team Team { get; init; }

	[CommandArgument(2, "<community>")]
	[Description("Slug of the community to publish the package to")]
	public required string Community { get; init; }

	[CommandOption($"{CATEGORY_OPTION} <CATEGORY>")]
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

		if (!Team.IsValid())
			return ValidationResult.Error($"'{Team}' is not a valid package team.");

		if (string.IsNullOrWhiteSpace(Community))
			return ValidationResult.Error("Community cannot be empty.");

		if (Categories != null)
		{
			var invalidCategories = Categories.Where(string.IsNullOrWhiteSpace).ToArray();

			if (invalidCategories.Length > 0)
			{
				var list = string.Join(", ", invalidCategories.Select(d => $"'{d}'"));
				return ValidationResult.Error(
					$"'{CATEGORY_OPTION}' contains invalid value(s): {list}"
				);
			}
		}

		return base.Validate();
	}
}
