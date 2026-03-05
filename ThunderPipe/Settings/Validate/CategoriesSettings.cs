using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings.Validate;

/// <summary>
/// Settings used by <see cref="Commands.Validate.CategoriesCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class CategoriesSettings : BaseValidateSettings
{
	[CommandArgument(0, "<community>")]
	[Description("Slug of the community to validate categories against")]
	public required string Community { get; init; }

	[CommandArgument(1, "<categories>")]
	[Description("Slugs of the categories to validate")]
	public required string[] Categories { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Community))
			return ValidationResult.Error("Community cannot be empty.");

		if (Categories.Any(string.IsNullOrWhiteSpace))
			return ValidationResult.Error("Categories contain an empty item.");

		return base.Validate();
	}
}
