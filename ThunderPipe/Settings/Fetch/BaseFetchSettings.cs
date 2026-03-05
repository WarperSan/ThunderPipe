using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings.Fetch;

/// <summary>
/// Settings used by any fetching command
/// </summary>
internal abstract class BaseFetchSettings : BaseCommandSettings
{
	private const string HOST_OPTION = "--host";

	[CommandOption(HOST_OPTION)]
	[Description("URL of the Thunderstore server")]
	[DefaultValue("https://thunderstore.io")]
	[TypeConverter(typeof(UriTypeConverter))]
	public Uri? Host { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (Host == null)
			return ValidationResult.Error($"'{HOST_OPTION}' cannot be empty.");

		return base.Validate();
	}
}
