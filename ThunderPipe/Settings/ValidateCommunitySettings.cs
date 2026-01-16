using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ThunderPipe.Settings;

/// <summary>
/// Settings used by <see cref="CommunityCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ValidateCommunitySettings : ValidateSettings
{
	[CommandArgument(0, "<community>")]
	[Description("Community where the package will be published")]
	public required string Community { get; init; }

	/// <inheritdoc />
	public override ValidationResult Validate()
	{
		if (string.IsNullOrEmpty(Community))
			return ValidationResult.Error("Community cannot be empty.");

		return base.Validate();
	}
}
