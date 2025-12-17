using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;
using ThunderPipe.Validations;

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

		var validations = new List<BaseValidationRule>();

		if (!settings.IgnoreLocalValidation) { }

		if (settings.UseRemoteValidation)
		{
			validations.Add(new RemoteIconValidationRule(iconPath));
			validations.Add(new RemoteManifestValidationRule(manifestPath, "root"));
		}

		foreach (var validation in validations)
		{
			var error = await validation.Validate(builder, cancellationToken);

			if (error == null)
				continue;

			Log.Error(error);
			return 1;
		}

		Log.Success("All files are valid!");
		return 0;
	}
}
