using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Infrastructure.TypeConverters;
using ThunderPipe.Models.Internal;

namespace ThunderPipe.Settings.Create;

/// <summary>
/// Settings used by <see cref="Commands.Create.ManifestCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class ManifestSettings : BaseCreateSettings
{
	private const string DEPENDENCY_OPTION = "--dependency";

	[CommandArgument(0, "<name>")]
	[Description("Name of the package")]
	public required PackageName Name { get; init; }

	[CommandArgument(1, "<version>")]
	[Description("Version of the package")]
	public required PackageVersion Version { get; init; }

	[CommandOption("--description <DESCRIPTION>")]
	[Description("Short description of the package")]
	public string? Description { get; init; }

	[CommandOption("--website <WEBSITE>")]
	[Description("URL of the package's homepage or source repository")]
	[TypeConverter(typeof(UriTypeConverter))]
	public Uri? Website { get; set; }

	[CommandOption($"{DEPENDENCY_OPTION} <DEPENDENCY>")]
	[Description("Dependency string for a required package")]
	public PackageDependency[]? Dependencies { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Name.IsValid())
			return ValidationResult.Error($"'{Name}' is not a valid package name.");

		if (!Version.IsValid())
			return ValidationResult.Error($"'{Version}' is not a valid package version.");

		if (Dependencies != null)
		{
			var invalidDependencies = Dependencies.Where(d => !d.IsValid()).ToArray();

			if (invalidDependencies.Length > 0)
			{
				var list = string.Join(", ", invalidDependencies.Select(d => $"'{d}'"));
				return ValidationResult.Error(
					$"'{DEPENDENCY_OPTION}' contains invalid value(s): {list}"
				);
			}
		}

		return base.Validate();
	}
}
