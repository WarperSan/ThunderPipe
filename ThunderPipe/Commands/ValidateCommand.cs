using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

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

		var validations = new List<Func<Task<ValidationResult>>>();

		if (!settings.IgnoreLocalValidation) { }

		if (settings.UseRemoteValidation)
		{
			validations.Add(() => ValidateIconRemote(iconPath, builder, cancellationToken));
			validations.Add(() =>
				ValidateManifestRemote(manifestPath, settings.Author!, builder, cancellationToken)
			);
			//validations.Add(() => ValidateReadmeRemote(readmePath, builder, cancellationToken));
		}

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

	private sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);

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
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var response = await ThunderstoreAPI.ValidateIcon(path, builder, cancellationToken);

		if (response == null)
			return new ValidationResult(false, ["Failed to validate icon remotely."]);

		if (response.DataErrors != null)
			return new ValidationResult(false, response.DataErrors);

		if (response.ValidationErrors != null)
			return new ValidationResult(false, response.ValidationErrors);

		if (response.Valid is null or false)
			return new ValidationResult(false, ["Icon was not marked as valid."]);

		return new ValidationResult(true, []);
	}

	private static async Task<ValidationResult> ValidateManifestRemote(
		string path,
		string author,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var response = await ThunderstoreAPI.ValidateManifest(
			path,
			author,
			builder,
			cancellationToken
		);

		if (response == null)
			return new ValidationResult(false, ["Failed to validate manifest remotely."]);

		if (response.DataErrors != null)
			return new ValidationResult(false, response.DataErrors);

		if (response.NamespaceErrors != null)
			return new ValidationResult(false, response.NamespaceErrors);

		if (response.ValidationErrors != null)
			return new ValidationResult(false, response.ValidationErrors);

		if (response.Valid is null or false)
			return new ValidationResult(false, ["Manifest was not marked as valid."]);

		return new ValidationResult(true, []);
	}

	private static async Task<ValidationResult> ValidateReadmeRemote(
		string path,
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var response = await ThunderstoreAPI.ValidateReadme(path, builder, cancellationToken);

		if (response == null)
			return new ValidationResult(false, ["Failed to validate README remotely."]);

		if (response.DataErrors != null)
			return new ValidationResult(false, response.DataErrors);

		if (response.ValidationErrors != null)
			return new ValidationResult(false, response.ValidationErrors);

		if (response.Valid is null or false)
			return new ValidationResult(false, ["README was not marked as valid."]);

		return new ValidationResult(true, []);
	}

	// ReSharper restore ConvertIfStatementToReturnStatement
	#endregion
}
