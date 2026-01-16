using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.DTOs;
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

		var uploadData = await ThunderstoreAPI.InitiateMultipartUpload(
			file,
			builder,
			cancellationToken
		);

		if (uploadData == null)
		{
			_logger.LogError("Failed to initiate upload.");
			return 1;
		}

		var fileSize = uploadData.FileMetadata.Size;
		var chunkCount = uploadData.UploadParts.Length;

		_logger.LogInformation(
			"Uploading '{File}' ({GetSizeString}) in {ChunkCount} chunks.",
			file,
			GetSizeString(fileSize),
			chunkCount
		);

		UploadPartResponse[] uploadedParts;

		try
		{
			uploadedParts = await ThunderstoreAPI.UploadParts(
				file,
				uploadData.UploadParts,
				cancellationToken
			);
		}
		catch (Exception e)
		{
			_logger.LogError(e.Message);

			await ThunderstoreAPI.AbortMultipartUpload(
				uploadData.FileMetadata.UUID,
				builder,
				cancellationToken
			);
			return 1;
		}

		if (uploadedParts.Length != chunkCount)
		{
			_logger.LogError("Failed to upload parts.");
			return 1;
		}

		var finishedUpload = await ThunderstoreAPI.FinishMultipartUpload(
			uploadData.FileMetadata.UUID,
			uploadedParts,
			builder,
			cancellationToken
		);

		if (!finishedUpload)
		{
			_logger.LogError("Failed to finish upload.");
			return 1;
		}

		_logger.LogInformation("Successfully finalized the upload.");

		var releasedPackage = await ThunderstoreAPI.SubmitPackage(
			settings.Team,
			settings.Community,
			settings.Categories ?? [],
			settings.HasNsfw,
			uploadData.FileMetadata.UUID,
			builder,
			cancellationToken
		);

		if (releasedPackage == null)
		{
			_logger.LogError("Failed to submit package.");
			return 1;
		}

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
