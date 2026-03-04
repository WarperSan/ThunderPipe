using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings.Fetch;

/// <summary>
/// Settings used by any fetching command
/// </summary>
public abstract class BaseSettings : BaseCommandSettings
{
	[CommandOption("--repository")]
	[Description("URL of the server hosting the package")]
	[DefaultValue("https://thunderstore.io")]
	[TypeConverter(typeof(UriTypeConverter))]
	public Uri? Repository { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (Repository == null)
			return ValidationResult.Error("Repository cannot be empty.");

		return base.Validate();
	}
}
