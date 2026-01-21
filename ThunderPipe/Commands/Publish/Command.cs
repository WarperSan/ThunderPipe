using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Publish;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class Command : AsyncCommand<Settings.Publish.Settings>
{
	private readonly ILogger<Command> _logger;

	public Command(ILogger<Command> logger)
	{
		_logger = logger;
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
		var uploadData = await client.InitiateMultipartUpload(file);

		var fileSize = uploadData.FileMetadata.Size;
		var chunkCount = uploadData.UploadParts.Length;

		_logger.LogInformation(
			"Uploading '{File}' ({GetSizeString}) in {ChunkCount} chunks.",
			file,
			GetSizeString(fileSize),
			chunkCount
		);

		Models.API.UploadPart.Response[] uploadedParts;

		try
		{
			uploadedParts = await client.UploadParts(file, uploadData.UploadParts);
		}
		catch (Exception)
		{
			await client.AbortMultipartUpload(uploadData.FileMetadata.UUID);
			throw;
		}

		if (uploadedParts.Length != chunkCount)
			throw new InvalidOperationException("Failed to upload every parts.");

		var finishedUpload = await client.FinishMultipartUpload(
			uploadData.FileMetadata.UUID,
			uploadedParts
		);

		if (!finishedUpload)
			throw new InvalidOperationException("Failed to finish upload.");

		_logger.LogInformation("Successfully finalized the upload.");

		var releasedPackage = await client.SubmitPackage(
			settings.Team,
			settings.Community,
			settings.Categories ?? [],
			settings.HasNsfw,
			uploadData.FileMetadata.UUID
		);

		_logger.LogInformation(
			"Successfully published '{VersionName}' v{VersionVersion}",
			releasedPackage.Version.Name,
			releasedPackage.Version.Version
		);

		_logger.LogInformation(
			"The package is now available at '{VersionDownloadURL}'.",
			releasedPackage.Version.DownloadURL
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
