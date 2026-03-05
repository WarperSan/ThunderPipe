using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

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
	public required string Community { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrEmpty(Community))
			return ValidationResult.Error("Community cannot be empty.");

		return base.Validate();
	}
}
