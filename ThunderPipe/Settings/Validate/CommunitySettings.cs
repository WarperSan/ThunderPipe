using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Settings.Validate;

/// <summary>
/// Settings used by <see cref="Commands.Validate.CommunityCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class CommunitySettings : BaseValidateSettings
{
	[CommandArgument(0, "<community>")]
	[Description("Slug of the community to validate")]
	public required Community Community { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (!Community.IsValid())
			return ValidationResult.Error($"'{Community}' is not a valid community.");

		return base.Validate();
	}
}
