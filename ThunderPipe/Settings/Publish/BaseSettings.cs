using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings.Publish;

/// <summary>
/// Settings used by any publishing command
/// </summary>
public abstract class BaseSettings : BaseCommandSettings
{
	[CommandOption("--token", true)]
	[Description("Authentication token used to publish the package")]
	public required string Token { get; init; }

	[CommandOption("--repository")]
	[Description("URL of the repository where to publish the package")]
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
