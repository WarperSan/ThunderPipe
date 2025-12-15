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

		var builder = new RequestBuilder().ToUrl(settings.Repository!).WithAuth(settings.Token);

		Log.WriteLine($"Publishing '[cyan]{file}[/]'");

		var uploadData = await ThunderstoreApi.InitiateMultipartUpload(
			file,
			builder.Copy(),
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

		var uploadedParts = await ThunderstoreApi.UploadParts(
			file,
			uploadData.UploadParts,
			cancellationToken
		);

		if (uploadedParts.Length != chunkCount)
		{
			Log.Error("Failed to upload parts.");
			return 1;
		}

		var finishedUpload = await ThunderstoreApi.FinishMultipartUpload(
			uploadData.FileMetadata.UUID,
			uploadedParts,
			builder.Copy(),
			cancellationToken
		);

		if (!finishedUpload)
		{
			Log.Error("Failed to finish upload.");
			return 1;
		}

		Log.WriteLine("Successfully finalized the upload.");

		var releasedPackage = await ThunderstoreApi.SubmitPackage(
			settings.Team,
			settings.Community,
			settings.Categories ?? [],
			settings.HasNsfw,
			uploadData.FileMetadata.UUID,
			builder.Copy(),
			cancellationToken
		);

		if (releasedPackage == null)
		{
			Log.Error("Failed to submit package.");
			return 1;
		}

		Log.WriteLine($"[lime]Successfully published '{file}'[/]");
		Log.WriteLine($"The package is now available at '[cyan][/]'.");

		return 0;
	}
}
