using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using ThunderPipe.Models;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PublishCommand : AsyncCommand<PublishSettings>
{
	/// <inheritdoc />
	protected override async Task<int> ExecuteAsync(CommandContext context, PublishSettings settings, CancellationToken cancellationToken)
	{
		// --- Settings ---
		var file = settings.File;
		var community = settings.Community;
		var categories = settings.Categories ?? [];
		
		var builder = new RequestBuilder()
		              .ToUrl(settings.Repository!)
		              .WithAuth(settings.Token);
		// ---

		// usermedia/initiate-upload/
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

		// Upload parts
		var uploadPartTasks = new List<Task<UploadPartModel?>>();

		await using (var stream = File.OpenRead(file))
		{
			foreach (var uploadPart in uploadData.UploadParts)
			{
				stream.Seek(uploadPart.Offset, SeekOrigin.Begin);

				var task = ThunderstoreApi.UploadPart(
					stream,
					uploadPart.PartNumber,
					uploadPart.Size,
					uploadPart.Url,
					cancellationToken
				);
				
				uploadPartTasks.Add(task);
			}
		}

		var uploadParts = (await Task.WhenAll(uploadPartTasks))
		                  .OfType<UploadPartModel>()
		                  .ToArray();

		if (uploadParts.Length != uploadData.UploadParts.Length)
		{
			await Console.Error.WriteLineAsync("Failed to upload parts.");
			return 1;
		}
		
		// usermedia/{uuid}/finish-upload/
		var finishedUpload = await ThunderstoreApi.FinishMultipartUpload(
			uploadData.FileMetadata.UUID,
			uploadParts,
			builder.Copy(),
			cancellationToken
		);

		if (!finishedUpload)
		{
			await Console.Error.WriteLineAsync("Failed to finish upload.");
			return 1;
		}

		// submission/submit/
		await ThunderstoreApi.SubmitPackage(
			"root",
			community,
			categories,
			false,
			uploadData.FileMetadata.UUID,
			builder.Copy(),
			cancellationToken
		);

		return 0;
	}
}