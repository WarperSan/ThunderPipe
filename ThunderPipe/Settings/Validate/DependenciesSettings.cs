using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Models.Internal;

namespace ThunderPipe.Settings.Validate;

/// <summary>
/// Settings used by <see cref="Commands.Validate.DependenciesCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class DependenciesSettings : BaseSettings
{
	[CommandArgument(0, "<dependencies>")]
	[Description("Dependency string to validate")]
	public required PackageDependency[] Dependencies { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		var invalidDependencies = Dependencies.Where(d => !d.IsValid()).ToArray();

		if (invalidDependencies.Length > 0)
		{
			var list = string.Join(", ", invalidDependencies.Select(d => $"'{d}'"));

			return ValidationResult.Error($"Dependencies contains invalid value(s): {list}");
		}

		return base.Validate();
	}
}
