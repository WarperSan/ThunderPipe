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
		var manifestPath = Path.GetFullPath(settings.ManifestPath!, settings.PackageFolder);

		var iconErrors = await ThunderstoreAPI.ValidateIcon(iconPath, builder, cancellationToken);

		if (iconErrors == null)
		{
			Log.Error("Failed to validate the icon remotely.");
			return 1;
		}

		if (iconErrors.DataErrors != null || iconErrors.ValidationErrors != null)
		{
			var errors = new List<string>();

			if (iconErrors.DataErrors != null)
				errors.AddRange(iconErrors.DataErrors);

			if (iconErrors.ValidationErrors != null)
				errors.AddRange(iconErrors.ValidationErrors);

			Log.Error(
				$"File at '{iconPath}' has resulted in these errors:\n\n- {string.Join("\n- ", errors).EscapeMarkup()}"
			);
			return 1;
		}

		var manifestErrors = await ThunderstoreAPI.ValidateManifest(
			manifestPath,
			"root",
			builder,
			cancellationToken
		);

		if (manifestErrors == null)
		{
			Log.Error("Failed to validate the manifest remotely.");
			return 1;
		}

		if (
			manifestErrors.FieldErrors != null
			|| manifestErrors.NamespaceErrors != null
			|| manifestErrors.ValidationErrors != null
		)
		{
			var errors = new List<string>();

			if (manifestErrors.FieldErrors != null)
				errors.AddRange(manifestErrors.FieldErrors);

			if (manifestErrors.NamespaceErrors != null)
				errors.AddRange(manifestErrors.NamespaceErrors);

			if (manifestErrors.ValidationErrors != null)
				errors.AddRange(manifestErrors.ValidationErrors);

			Log.Error(
				$"File at '{iconPath}' has resulted in these errors:\n\n- {string.Join("\n- ", errors).EscapeMarkup()}"
			);
			return 1;
		}

		return 0;
	}
}
