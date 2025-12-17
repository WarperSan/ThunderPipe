using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
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
		var builder = RequestBuilder.Create(settings.Token, settings.Repository!);

		var iconPath = Path.GetFullPath(settings.IconPath!, settings.PackageFolder);

		var iconErrors = await ThunderstoreAPI.ValidateIcon(iconPath, builder, cancellationToken);

		if (iconErrors == null)
		{
			Log.Error("Failed to validate the icon remotely.");
			return 1;
		}

		if (iconErrors.FieldErrors != null || iconErrors.NonFieldErrors != null)
		{
			var errors = new List<string>();

			if (iconErrors.FieldErrors != null)
				errors.AddRange(iconErrors.FieldErrors);

			if (iconErrors.NonFieldErrors != null)
				errors.AddRange(iconErrors.NonFieldErrors);

			Log.Error(
				$"Icon '{iconPath}' has resulted in these errors:\n\n- {string.Join("\n- ", errors).EscapeMarkup()}"
			);
			return 1;
		}

		return 0;
	}
}
