using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ValidateCommand : AsyncCommand<ValidateSettings>
{
	/// <inheritdoc />
	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		ValidateSettings settings,
		CancellationToken cancellationToken
	)
	{
		var builder = new RequestBuilder().ToUrl(settings.Repository!).WithAuth(settings.Token);

		await ThunderstoreAPI.ValidateIcon(
			"/home/warpersan/Projects/TypeScript/action-thunderstore-check/__tests__/assets/icons/wrong-type.png",
			//Path.Combine(settings.PackageFolder, "icon.png"),
			builder.Copy(),
			cancellationToken
		);
		return 0;
	}
}
