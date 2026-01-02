using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;
using ThunderPipe.Validations;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ValidateCommand : AsyncCommand<ValidateSettings>
{
	private readonly ILogger<ValidateCommand> _logger;

	public ValidateCommand(ILogger<ValidateCommand> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		ValidateSettings settings,
		CancellationToken cancellationToken
	)
	{
		_logger.LogInformation(
			"Starting to validate '{SettingsPackageFolder}'",
			settings.PackageFolder
		);

		var builder = new RequestBuilder().ToUri(settings.Repository!).WithAuth(settings.Token);

		var iconPath = Path.GetFullPath(settings.IconPath!, settings.PackageFolder);
		var manifestPath = Path.GetFullPath(settings.ManifestPath!, settings.PackageFolder);
		var readmePath = Path.GetFullPath(settings.ReadmePath!, settings.PackageFolder);

		var validations = new List<BaseValidationRule>();

		if (!settings.IgnoreLocalValidation) { }

		if (settings.UseRemoteValidation)
		{
			validations.Add(new RemoteIconValidationRule(iconPath));
			validations.Add(new RemoteManifestValidationRule(manifestPath, settings.Author!));
			//validations.Add(new RemoteReadmeValidationRule(readmePath));
		}

		if (validations.Count == 0)
		{
			_logger.LogError("No validation rule was applied.");
			return 1;
		}

		foreach (var validation in validations)
		{
			var error = await validation.Validate(builder, cancellationToken);

			if (error == null)
				continue;

			_logger.LogError(error);
			return 1;
		}

		_logger.LogInformation("All files are valid!");
		return 0;
	}
}
