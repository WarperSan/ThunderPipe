using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Infrastructure.TypeConverters;
using ThunderPipe.Models.Internal;
using ThunderPipe.Utils;

namespace ThunderPipe.Settings.Create;

/// <summary>
/// Settings used by <see cref="Commands.Create.ManifestCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class ManifestSettings : BaseCreateSettings
{
	[CommandArgument(0, "<name>")]
	[Description("Name of the package")]
	[TypeConverter(typeof(PackageNameTypeConverter))]
	public required PackageName Name { get; init; }

	[CommandArgument(1, "<version>")]
	[Description("Version of the package")]
	public required string Version { get; init; }

	[CommandOption("--description <DESCRIPTION>")]
	[Description("Short description of the package")]
	public string? Description { get; init; }

	[CommandOption("--website <WEBSITE>")]
	[Description("URL of the package's homepage or source repository")]
	[TypeConverter(typeof(UriTypeConverter))]
	public Uri? Website { get; set; }

	[CommandOption("--dependency <DEPENDENCY>")]
	[Description("Other package needed by the package")]
	public string[]? Dependencies { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Name.IsValid())
			return ValidationResult.Error($"'{Name}' is not a valid package name.");

		if (string.IsNullOrEmpty(Version))
			return ValidationResult.Error("Version cannot be empty.");

		if (!RegexHelper.IsVersionValid(Version))
			return ValidationResult.Error("Version contains illegal characters.");

		if (Dependencies != null && !Dependencies.All(RegexHelper.IsDependencyValid))
			return ValidationResult.Error("Dependencies contain item(s) with illegal characters.");

		return base.Validate();
	}
}
