using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PublishCommand : AsyncCommand<PublishSettings>
{
	/// <inheritdoc />
	protected override async Task<int> ExecuteAsync(CommandContext context, PublishSettings settings, CancellationToken cancellationToken)
	{
		var file = settings.File;

		var builder = new RequestBuilder()
		              .ToUrl(settings.Repository!)
		              .WithAuth(settings.Token);

		var uploadData = await ThunderstoreApi.InitiateMultipartUpload(
			file,
			builder.Copy(),
			cancellationToken
		);

		if (uploadData == null)
		{
			await Console.Error.WriteLineAsync("Failed to initiate upload.");
			return 1;
		}

		var uploadedParts = await ThunderstoreApi.UploadParts(
			file,
			uploadData.UploadParts,
			cancellationToken
		);

		if (uploadedParts.Length != uploadData.UploadParts.Length)
		{
			await Console.Error.WriteLineAsync("Failed to upload parts.");
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
			await Console.Error.WriteLineAsync("Failed to finish upload.");
			return 1;
		}
		
		await ThunderstoreApi.SubmitPackage(
			settings.Team,
			settings.Community,
			settings.Categories ?? [],
			settings.HasNsfw,
			uploadData.FileMetadata.UUID,
			builder.Copy(),
			cancellationToken
		);

		return 0;
	}
}