using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by <see cref="ValidateCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ValidateSettings : CommandSettings
{
	[CommandArgument(0, "<package-folder>")]
	[Description("Folder containg the package's files")]
	public required string PackageFolder { get; init; }

	[CommandOption("--token")]
	[Description("Authentication token used to publish the package")]
	public required string Token { get; init; }

	[CommandOption("--disable-local|--no-local")]
	[Description("Determines if local validation will be ignored")]
	[DefaultValue(false)]
	public bool IgnoreLocalValidation { get; init; }

	[CommandOption("--enable-remote|--remote")]
	[Description("Determines if remote validation rules will be used")]
	[DefaultValue(false)]
	public bool UseRemoteValidation { get; init; }

	[CommandOption("--repository")]
	[Description("URL of the server hosting the package")]
	[DefaultValue("https://thunderstore.io")]
	public string? Repository { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Directory.Exists(PackageFolder))
			return ValidationResult.Error($"No folder was found at '{PackageFolder}'.");

		if (IgnoreLocalValidation && !UseRemoteValidation)
			return ValidationResult.Error("At least one validation source must be used.");

		if (UseRemoteValidation && string.IsNullOrWhiteSpace(Repository))
			return ValidationResult.Error(
				"If remote validation is used, a repository must be specified."
			);

		if (UseRemoteValidation && string.IsNullOrWhiteSpace(Token))
			return ValidationResult.Error(
				"If remote validation is used, a token must be specified."
			);

		return base.Validate();
	}
}
