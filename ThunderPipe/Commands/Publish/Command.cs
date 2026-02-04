using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Models.Domain.MultipartUpload;
using ThunderPipe.Services.Interfaces;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Publish;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class Command : AsyncCommand<Settings.Publish.Settings>
{
	private readonly ILogger<Command> _logger;
	private readonly IFileSystem _fileSystem;

	public Command(ILogger<Command> logger, IFileSystem fileSystem)
	{
		_logger = logger;
		_fileSystem = fileSystem;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		Settings.Publish.Settings settings,
		CancellationToken cancellationToken
	)
	{
		var file = settings.File;

		_logger.LogInformation("Starting to publish '{File}'.", file);

		var builder = new RequestBuilder().ToUri(settings.Repository!).WithAuth(settings.Token);

		_logger.LogInformation("Publishing '{File}'", file);

		using var client = new PublishApiClient(builder, new HttpClient(), cancellationToken);
		var uploadSession = await client.InitiateMultipartUpload(file, _fileSystem);

		_logger.LogInformation(
			"Uploading '{File}' ({GetSizeString}) in {ChunkCount} chunks.",
			file,
			GetSizeString(_fileSystem.GetSize(file)),
			uploadSession.Parts.Count
		);

		IReadOnlyCollection<UploadPart> uploadedParts;

		try
		{
			uploadedParts = await client.UploadParts(file, uploadSession.Parts, _fileSystem);
		}
		catch (Exception)
		{
			await client.AbortMultipartUpload(uploadSession.UUID);
			throw;
		}

		if (uploadedParts.Count != uploadSession.Parts.Count)
		{
			await client.AbortMultipartUpload(uploadSession.UUID);
			throw new InvalidOperationException("Failed to upload every parts.");
		}

		var finishedUpload = await client.FinishMultipartUpload(uploadSession.UUID, uploadedParts);

		if (!finishedUpload)
			throw new InvalidOperationException("Failed to finish upload.");

		_logger.LogInformation("Successfully finalized the upload.");

		var releasedPackage = await client.SubmitPackage(
			settings.Team,
			settings.Community,
			settings.Categories ?? [],
			settings.HasNsfw,
			uploadSession.UUID
		);

		_logger.LogInformation(
			"Successfully published '{VersionName}' v{VersionVersion}",
			releasedPackage.Name,
			releasedPackage.Version
		);

		_logger.LogInformation(
			"The package is now available at '{VersionDownloadURL}'.",
			releasedPackage.DownloadURL
		);

		return 0;
	}

	/// <summary>
	/// Formats a byte size into readable text
	/// </summary>
	private static string GetSizeString(long byteSize)
	{
		double finalSize = byteSize;

		string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
		var suffixIndex = 0;

		while (finalSize >= 1024 && suffixIndex < suffixes.Length)
		{
			finalSize /= 1024;
			suffixIndex++;
		}

		return $"{finalSize:F2} {suffixes[suffixIndex]}";
	}
}
