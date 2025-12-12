using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by <see cref="PublishCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PublishSettings : CommandSettings
{
	[CommandArgument(0, "<file>")]
	[Description("Path to the package file to publish")]
	public required string File { get; init; }

	[CommandArgument(1, "<community>")]
	[Description("Community where to publish the package")]
	public required string Community { get; init; }

	[CommandOption("--token", true)]
	[Description("Authentication token used to publish the package")]
	public required string Token { get; init; }

	[CommandOption("--repository")]
	[Description("URL of the server hosting the package")]
	[DefaultValue("https://thunderstore.io")]
	public string? Repository { get; init; }

	[CommandOption("--categories <VALUES>")]
	[Description("Categories used to label this package")]
	public string[]? Categories { get; init; }
	
	[CommandOption("--has-nsfw|--nsfw")]
	[Description("Determines if this package has NSFW content")]
	[DefaultValue(false)]
	public bool HasNsfw { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!System.IO.File.Exists(File))
			return ValidationResult.Error($"No file was found at '{File}'.");

		if (string.IsNullOrWhiteSpace(Community))
			return ValidationResult.Error("Community cannot be empty.");

		if (string.IsNullOrWhiteSpace(Token))
			return ValidationResult.Error("Token cannot be empty.");

		if (!Uri.TryCreate(Repository, UriKind.Absolute, out var hostUri)
		    || hostUri.Scheme != Uri.UriSchemeHttp && hostUri.Scheme != Uri.UriSchemeHttps)
			return ValidationResult.Error($"Repository '{Repository}' is not a valid URL.");

		if (Categories != null && Categories.Any(string.IsNullOrEmpty))
			return ValidationResult.Error("Categories contains an empty item.");

		return base.Validate();
	}
}