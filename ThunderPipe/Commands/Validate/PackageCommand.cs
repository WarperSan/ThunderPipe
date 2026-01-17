using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Settings.Validate;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PackageCommand : AsyncCommand<PackageSettings>
{
	private readonly ILogger<PackageCommand> _logger;

	public PackageCommand(ILogger<PackageCommand> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		PackageSettings settings,
		CancellationToken cancellationToken
	)
	{
		_logger.LogInformation(
			"Starting to validate '{SettingsPackageFolder}'",
			settings.PackageFolder
		);

		var builder = new RequestBuilder().ToUri(settings.Repository!).WithAuth(settings.Token);
		var client = new ValidationApiClient(builder, cancellationToken);

		var iconPath = Path.GetFullPath(settings.IconPath!, settings.PackageFolder);
		var manifestPath = Path.GetFullPath(settings.ManifestPath!, settings.PackageFolder);
		var readmePath = Path.GetFullPath(settings.ReadmePath!, settings.PackageFolder);

		var validations = new List<Func<Task<ValidationResult>>>
		{
			() => ValidateIconRemote(iconPath, client),
			() => ValidateManifestRemote(manifestPath, settings.Team!, client),
			//() => ValidateReadmeRemote(readmePath, client)
		};

		//validations.Add();

		if (validations.Count == 0)
		{
			_logger.LogError("No validation rule was applied.");
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

			_logger.LogError(output.ToString());
			return 1;
		}

		_logger.LogInformation("All files are valid!");
		return 0;
	}

	#region Validation Rules
	// ReSharper disable ConvertIfStatementToReturnStatement

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

	private static async Task<ValidationResult> ValidateIconRemote(
		string path,
		ValidationApiClient client
	)
	{
		var errors = await client.IsIconValid(path);

		return new ValidationResult(errors.Count == 0, errors);
	}

	private static async Task<ValidationResult> ValidateManifestRemote(
		string path,
		string team,
		ValidationApiClient client
	)
	{
		var errors = await client.IsManifestValid(path, team);

		return new ValidationResult(errors.Count == 0, errors);
	}

	private static async Task<ValidationResult> ValidateReadmeRemote(
		string path,
		ValidationApiClient client
	)
	{
		var errors = await client.IsReadmeValid(path);

		return new ValidationResult(errors.Count == 0, errors);
	}

	// ReSharper restore ConvertIfStatementToReturnStatement
	#endregion
}
