using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings.Publish;

/// <summary>
/// Settings used by any publishing command
/// </summary>
internal abstract class BasePublishSettings : BaseCommandSettings
{
	private const string TOKEN_OPTION = "--token";
	private const string HOST_OPTION = "--host";

	[CommandOption(TOKEN_OPTION, true)]
	[Description("Service account API token for authentication")]
	public required string Token { get; init; }

	[CommandOption(HOST_OPTION)]
	[Description("URL of the Thunderstore server to publish to")]
	[DefaultValue(Core.Constants.DEFAULT_HOST)]
	[TypeConverter(typeof(UriTypeConverter))]
	public Uri? Host { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Token))
			return ValidationResult.Error($"'{TOKEN_OPTION}' cannot be empty.");

		if (Host == null)
			return ValidationResult.Error($"'{HOST_OPTION}' cannot be empty.");

		return base.Validate();
	}
}
