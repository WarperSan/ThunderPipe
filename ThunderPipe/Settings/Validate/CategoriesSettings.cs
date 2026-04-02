using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Core.Models.API;

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
	public required Community Community { get; init; }

	[CommandArgument(1, "<categories>")]
	[Description("Slugs of the categories to validate")]
	public required Category[] Categories { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Community.IsValid())
			return ValidationResult.Error($"'{Community}' is not a valid community.");

		var invalidCategories = Categories.Where(d => !d.IsValid()).ToArray();

		if (invalidCategories.Length > 0)
		{
			var list = string.Join(", ", invalidCategories.Select(d => $"'{d}'"));

			return ValidationResult.Error($"Invalid categories: {list}");
		}

		return base.Validate();
	}
}
