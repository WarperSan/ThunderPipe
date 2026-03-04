using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands.Fetch;

namespace ThunderPipe.Settings.Fetch;

/// <summary>
/// Settings used by <see cref="LatestVersionCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class LatestVersionSettings : BaseSettings
{
	[CommandArgument(0, "<team>")]
	[Description("Team owning the package")]
	public required string Team { get; init; }

	[CommandArgument(1, "<name>")]
	[Description("Name of the package")]
	public required string Name { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Team))
			return ValidationResult.Error("Team must be specified.");

		if (string.IsNullOrWhiteSpace(Name))
			return ValidationResult.Error("Name must be specified.");

		return base.Validate();
	}
}
