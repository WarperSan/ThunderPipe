using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spectre.Console.Cli;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Settings.Create;

namespace ThunderPipe.Commands.Create;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ManifestCommand : BaseCommand<ManifestSettings>
{
	private readonly IFileSystem _fileSystem;

	public ManifestCommand(ILogger logger, IFileSystem fileSystem)
		: base(logger)
	{
		_fileSystem = fileSystem;
	}

	private record ManifestModel
	{
		[JsonProperty("name")]
		[JsonRequired]
		public required string Name { get; init; }

		[JsonProperty("description")]
		[JsonRequired]
		public required string Description { get; init; }

		[JsonProperty("version_number")]
		[JsonRequired]
		public required string Version { get; init; }

		[JsonProperty("website_url")]
		[JsonRequired]
		public required string Website { get; init; }

		[JsonProperty("dependencies")]
		[JsonRequired]
		public required string[] Dependencies { get; init; }
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		ManifestSettings settings,
		CancellationToken cancellationToken
	)
	{
		var path = Path.Combine(settings.OutputDirectory!, "manifest.json");

		var data = new ManifestModel
		{
			Name = settings.Name,
			Description = settings.Description ?? "",
			Version = settings.Version,
			Website = settings.Website?.ToString() ?? "",
			Dependencies = settings.Dependencies?.Select(d => d.ToString()).ToArray() ?? [],
		};

		var json = JsonConvert.SerializeObject(data, Formatting.Indented);

		Logger.LogDebug("Writing content to '{Path}':\n{Content}", path, json);

		await _fileSystem.WriteAllTextAsync(path, json, cancellationToken);

		Logger.LogInformation("File wrote to '{Path}'.", path);
		return 0;
	}
}
