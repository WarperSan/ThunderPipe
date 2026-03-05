using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings.Validate;

/// <summary>
/// Settings used by any validation command
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal abstract class BaseValidateSettings : BaseCommandSettings
{
	private const string HOST_OPTION = "--host";

	[CommandOption(HOST_OPTION)]
	[Description("URL of the Thunderstore server to validate against")]
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
