using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PublishCommand : AsyncCommand<PublishSettings>
{
	/// <inheritdoc />
	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		PublishSettings settings,
		CancellationToken cancellationToken
	)
	{
		var file = settings.File;

		var builder = RequestBuilder.Create(settings.Token, settings.Repository!);

		Log.WriteLine($"Publishing '[cyan]{file}[/]'");

		var uploadData = await ThunderstoreAPI.InitiateMultipartUpload(
			file,
			builder,
			cancellationToken
		);

		if (uploadData == null)
		{
			Log.Error("Failed to initiate upload.");
			return 1;
		}

		var fileSize = uploadData.FileMetadata.Size;
		var chunkCount = uploadData.UploadParts.Length;
		Log.WriteLine(
			$"Uploading '[cyan]{file}[/]' ({Log.GetSizeString(fileSize)}) in {chunkCount} chunks."
		);

		var uploadedParts = await ThunderstoreAPI.UploadParts(
			file,
			uploadData.UploadParts,
			cancellationToken
		);

		if (uploadedParts.Length != chunkCount)
		{
			Log.Error("Failed to upload parts.");
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
			Log.Error("Failed to finish upload.");
			return 1;
		}

		Log.WriteLine("Successfully finalized the upload.");

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
			Log.Error("Failed to submit package.");
			return 1;
		}

		Log.WriteLine(
			$"[lime]Successfully published '{releasedPackage.Version.Name}' v{releasedPackage.Version.Version}[/]"
		);
		Log.WriteLine(
			$"The package is now available at '[link={releasedPackage.Version.DownloadURL}]{releasedPackage.Version.Name}[/]'."
		);

		return 0;
	}
}
