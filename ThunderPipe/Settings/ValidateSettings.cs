using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by <see cref="ValidateCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ValidateSettings : CommandSettings
{
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
		if (IgnoreLocalValidation && !UseRemoteValidation)
			return ValidationResult.Error("At least one validation source must be used.");

		if (UseRemoteValidation && string.IsNullOrWhiteSpace(Repository))
			return ValidationResult.Error(
				"If remote validation is used, a repository must be specified."
			);

		return base.Validate();
	}
}
