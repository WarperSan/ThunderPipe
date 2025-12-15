using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using ThunderPipe.Settings;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ValidateCommand : AsyncCommand<ValidateSettings>
{
	/// <inheritdoc />
	protected override Task<int> ExecuteAsync(
		CommandContext context,
		ValidateSettings settings,
		CancellationToken cancellationToken
	)
	{
		return 0;
	}
}
