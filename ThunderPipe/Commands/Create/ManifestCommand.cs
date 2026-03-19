using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Settings.Create;

namespace ThunderPipe.Commands.Create;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ManifestCommand : BaseCommand<ManifestSettings>
{
	private readonly CreationService _service;

	public ManifestCommand(ILogger logger, IFileSystem fileSystem)
		: base(logger)
	{
		_service = new CreationService(fileSystem, logger);
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		ManifestSettings settings,
		CancellationToken cancellationToken
	)
	{
		var package = new PackageManifest
		{
			Name = settings.Name,
			Description = settings.Description ?? "",
			Version = settings.Version,
			Website = settings.Website?.ToString() ?? "",
			Dependencies = settings.Dependencies ?? [],
		};

		await _service.CreateManifest(package, settings.OutputDirectory!, cancellationToken);

		return 0;
	}
}
