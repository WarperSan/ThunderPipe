using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Commands.Publish;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PackageCommand : BaseCommand<Settings.Publish.PackageSettings>
{
	private readonly IFileSystem _fileSystem;

	public PackageCommand(ILogger logger, IFileSystem fileSystem)
		: base(logger)
	{
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

		Logger.LogInformation("Starting to publish '{File}'.", file);

		var builder = new RequestBuilder().ToUri(settings.Host!);

		Logger.LogInformation("Publishing '{File}'", file);

		var service = new PublicationService(builder, _fileSystem, Logger);

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

		Logger.LogInformation(
			"Successfully published '{VersionName}' v{VersionVersion}",
			package.Name,
			package.Version
		);

		Logger.LogInformation(
			"The package is now available at '{VersionDownloadURL}'.",
			package.DownloadURL
		);

		return 0;
	}
}
