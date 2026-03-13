using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Models.Web.MultipartUpload;
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

		var builder = new RequestBuilder().ToUri(settings.Host!).WithAuth(settings.Token);

		Logger.LogInformation("Publishing '{File}'", file);

		using var client = new PublishApiClient();
		client.Builder = builder;
		client.CancellationToken = cancellationToken;
		client.Logger = Logger;

		var uploadSession = await client.InitiateMultipartUpload(file, _fileSystem);

		Logger.LogInformation(
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

		Logger.LogInformation("Successfully finalized the upload.");

		var releasedPackage = await client.SubmitPackage(
			settings.Team,
			settings.Community,
			settings.Categories ?? [],
			settings.HasNsfw,
			uploadSession.UUID
		);

		Logger.LogInformation(
			"Successfully published '{VersionName}' v{VersionVersion}",
			releasedPackage.Name,
			releasedPackage.Version
		);

		Logger.LogInformation(
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
