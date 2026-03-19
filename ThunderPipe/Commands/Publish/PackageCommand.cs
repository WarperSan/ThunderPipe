using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Commands.Publish;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PackageCommand : AsyncCommand<Settings.Publish.PackageSettings>
{
	private readonly ILogger _logger;
	private readonly IFileSystem _fileSystem;

	public PackageCommand(ILogger logger, IFileSystem fileSystem)
	{
		_logger = logger;
		_fileSystem = fileSystem;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		Settings.Publish.PackageSettings settings,
		CancellationToken cancellationToken
	)
	{
		var file = settings.File;

		_logger.LogInformation("Starting to publish '{File}'.", file);

		var builder = new RequestBuilder().ToUri(settings.Host!);

		_logger.LogInformation("Publishing '{File}'", file);

		var service = new PublicationService(builder, _fileSystem, _logger);

		var package = await service.PublishPackage(
			file,
			settings.Team,
			[settings.Community],
			new Dictionary<Community, IEnumerable<Category>>
			{
				[settings.Community] = settings.Categories ?? [],
			},
			settings.HasNsfw,
			settings.Token,
			cancellationToken
		);

		_logger.LogInformation(
			"Successfully published '{VersionName}' v{VersionVersion}",
			package.Name,
			package.Version
		);

		_logger.LogInformation(
			"The package is now available at '{VersionDownloadURL}'.",
			package.DownloadURL
		);

		return 0;
	}
}
