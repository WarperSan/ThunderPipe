using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Commands.Fetch;
using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Settings.Fetch;

/// <summary>
/// Settings used by <see cref="LatestVersionCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class LatestVersionSettings : BaseFetchSettings
{
	[CommandArgument(0, "<team>")]
	[Description("Team that owns the package")]
	public required PackageTeam Team { get; init; }

	[CommandArgument(1, "<name>")]
	[Description("Name of the package")]
	public required PackageName Name { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Team.IsValid())
			return ValidationResult.Error($"'{Team}' is not a valid package team.");

		if (!Name.IsValid())
			return ValidationResult.Error($"'{Name}' is not a valid package name.");

		return base.Validate();
	}
}
