using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by <see cref="DependenciesCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ValidateDependenciesSettings : ValidateSettings
{
	[CommandArgument(0, "<dependencies>")]
	[Description("Dependencies needed by the package")]
	public required string[] Dependencies { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (Dependencies.Any(string.IsNullOrEmpty))
			return ValidationResult.Error("Dependencies contain an empty item.");

		return base.Validate();
	}
}
