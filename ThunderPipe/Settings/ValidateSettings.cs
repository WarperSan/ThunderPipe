using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by any validation command
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class ValidateSettings : CommandSettings
{
	[CommandOption("--token")]
	[Description("Authentication token used to publish the package")]
	public required string Token { get; init; }

	[CommandOption("--repository")]
	[Description("URL of the server hosting the package")]
	[DefaultValue("https://thunderstore.io")]
	[TypeConverter(typeof(UriTypeConverter))]
	public Uri? Repository { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Token))
			return ValidationResult.Error("Token cannot be empty.");

		if (Repository == null)
			return ValidationResult.Error("Repository cannot be empty.");

		return base.Validate();
	}
}
