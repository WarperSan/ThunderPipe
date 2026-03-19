using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;
using ThunderPipe.Settings.Validate;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PackageCommand : AsyncCommand<PackageSettings>
{
	private readonly IFileSystem _fileSystem;
	private readonly ILogger _logger;

	public PackageCommand(ILogger logger, IFileSystem fileSystem)
	{
		_logger = logger;
		_fileSystem = fileSystem;
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

		var iconPath = Path.GetFullPath(settings.IconPath!, settings.PackageFolder);
		var manifestPath = Path.GetFullPath(settings.ManifestPath!, settings.PackageFolder);
		var readmePath = Path.GetFullPath(settings.ReadmePath!, settings.PackageFolder);

		var builder = new RequestBuilder().ToUri(settings.Host!);
		var service = new ValidationService(builder, _fileSystem, _logger);

		var errors = await service.ValidatePackage(
			settings.Team,
			iconPath,
			manifestPath,
			readmePath,
			settings.Token,
			cancellationToken
		);

		if (errors.Count > 0)
		{
			var output = new StringBuilder();

			output.AppendLine("Validation failed:");
			output.Append("- ");
			output.AppendJoin("\n- ", errors);

			_logger.LogError("{Output}", output.ToString());
			return 1;
		}

		_logger.LogInformation("All files are valid!");
		return 0;
	}
}
