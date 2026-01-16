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
public sealed class CategoriesSettings : BaseSettings
{
	[CommandArgument(0, "<community>")]
	[Description("Community where the package will be published")]
	public required string Community { get; init; }

	[CommandOption("--category <CATEGORY>")]
	[Description("Categories that will be used to label the package")]
	public string[]? Categories { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Community))
			return ValidationResult.Error("Community cannot be empty.");

		if (Categories == null || Categories.Length == 0)
			return ValidationResult.Error("At least one category must be specified.");

		if (Categories.Any(string.IsNullOrEmpty))
			return ValidationResult.Error("Categories contain an empty item.");

		return base.Validate();
	}
}
