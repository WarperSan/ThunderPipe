using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;
using ThunderPipe.Settings.Validate;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PackageCommand : BaseCommand<PackageSettings>
{
	private readonly IFileSystem _fileSystem;

	/// <inheritdoc />
	public PackageCommand(ILogger logger, IFileSystem fileSystem)
		: base(logger)
	{
		_fileSystem = fileSystem;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		PackageSettings settings,
		CancellationToken cancellationToken
	)
	{
		Logger.LogInformation(
			"Starting to validate '{SettingsPackageFolder}'",
			settings.PackageFolder
		);

		var builder = new RequestBuilder().ToUri(settings.Host!).WithAuth(settings.Token);
		var client = new ValidationApiClient();
		client.Builder = builder;
		client.Logger = Logger;

		var iconPath = Path.GetFullPath(settings.IconPath!, settings.PackageFolder);
		var manifestPath = Path.GetFullPath(settings.ManifestPath!, settings.PackageFolder);
		var readmePath = Path.GetFullPath(settings.ReadmePath!, settings.PackageFolder);

		var validations = new List<Func<Task<ValidationResult>>>
		{
			async () =>
			{
				var errors = await client.IsIconValid(iconPath, _fileSystem, cancellationToken);

				return new ValidationResult(errors.Count == 0, errors);
			},
			async () =>
			{
				var errors = await client.IsManifestValid(
					manifestPath,
					settings.Team,
					_fileSystem,
					cancellationToken
				);

				return new ValidationResult(errors.Count == 0, errors);
			},
			/*async () =>
			{
				var errors = await client.IsReadmeValid(readmePath, _fileSystem,
			   cancellationToken);

				return new ValidationResult(errors.Count == 0, errors);
			},*/
		};

		if (validations.Count == 0)
		{
			Logger.LogError("No validation rule was applied.");
			return 1;
		}

		var errors = await AnsiConsole
			.Progress()
			.StartAsync(ctx => RunValidations(validations, ctx));

		if (errors.Count > 0)
		{
			var output = new StringBuilder();

			output.AppendLine("Validation failed:");
			output.Append("- ");
			output.AppendJoin("\n- ", errors);

			Logger.LogError("{Output}", output.ToString());
			return 1;
		}

		Logger.LogInformation("All files are valid!");
		return 0;
	}

	#region Validation Rules

	private sealed record ValidationResult(bool IsValid, IEnumerable<string> Errors);

	private static async Task<List<string>> RunValidations(
		List<Func<Task<ValidationResult>>> validations,
		ProgressContext ctx
	)
	{
		var errors = new List<string>();

		var validationProgress = ctx.AddTask("Run validations", maxValue: validations.Count);

		foreach (var validation in validations)
		{
			var result = await validation.Invoke();

			validationProgress.Increment(1);

			Thread.Sleep(20);

			if (result.IsValid)
				continue;

			errors.AddRange(result.Errors);
		}

		return errors;
	}

	#endregion
}
