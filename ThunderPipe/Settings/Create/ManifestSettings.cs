using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Utils;

namespace ThunderPipe.Settings.Create;

/// <summary>
/// Settings used by <see cref="Commands.Create.ManifestCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ManifestSettings : BaseSettings
{
	[CommandArgument(0, "<name>")]
	[Description("Name of the package")]
	public required string Name { get; init; }

	[CommandArgument(1, "<version>")]
	[Description("Version of the package")]
	public required string Version { get; init; }

	[CommandOption("--description <DESCRIPTION>")]
	[Description("Description of the package")]
	public string? Description { get; init; }

	[CommandOption("--website <WEBSITE>")]
	[Description("URL of the package's website")]
	[TypeConverter(typeof(UriTypeConverter))]
	public Uri? Website { get; set; }

	[CommandOption("--dependency <DEPENDENCY>")]
	[Description("Dependencies needed by the package")]
	public string[]? Dependencies { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrEmpty(Name))
			return ValidationResult.Error("Name cannot be empty.");

		if (!RegexHelper.IsNameValid(Name))
			return ValidationResult.Error("Name contains illegal characters.");

		if (string.IsNullOrEmpty(Version))
			return ValidationResult.Error("Version cannot be empty.");

		if (!RegexHelper.IsVersionValid(Version))
			return ValidationResult.Error("Version contains illegal characters.");

		if (Dependencies != null && !Dependencies.All(RegexHelper.IsDependencyValid))
			return ValidationResult.Error("Dependencies contain item(s) with illegal characters.");

		return base.Validate();
	}
}
