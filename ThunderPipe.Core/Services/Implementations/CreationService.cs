using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Interfaces;

namespace ThunderPipe.Core.Services.Implementations;

public class CreationService : ICreationService
{
	private readonly IFileSystem _fileSystem;
	private readonly ILogger _logger;

	public CreationService(IFileSystem fileSystem, ILogger logger)
	{
		_fileSystem = fileSystem;
		_logger = logger;
	}

	/// <inheritdoc />
	public async Task CreateManifest(
		PackageManifest manifest,
		string destination,
		CancellationToken cancellationToken
	)
	{
		var path = Path.Combine(destination, "manifest.json");

		var json = JsonConvert.SerializeObject(manifest, Formatting.Indented);

		_logger.LogDebug("Writing content to '{Path}':\n{Content}", path, json);

		await _fileSystem.WriteAllTextAsync(path, json, cancellationToken);

		_logger.LogInformation("File wrote to '{Path}'.", path);
	}
}
