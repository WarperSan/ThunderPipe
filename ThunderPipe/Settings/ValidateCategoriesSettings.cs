using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by <see cref="ValidateCategoriesCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ValidateCategoriesSettings : ValidateSettings
{
	[CommandArgument(0, "<categories>")]
	[Description("Categories that will be used to label this package")]
	public required string[] Categories { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (Categories.Length == 0)
			return ValidationResult.Error("At least one category must be specified.");

		if (Categories.Any(string.IsNullOrEmpty))
			return ValidationResult.Error("Categories contains an empty item.");

		return base.Validate();
	}
}
