using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using ThunderPipe.Models;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PublishCommand : AsyncCommand<PublishCommand.Settings>
{
	/// <summary>
	/// Settings used by <see cref="PublishCommand"/>
	/// </summary>
	[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public sealed class Settings : CommandSettings
	{
		[CommandArgument(0, "<file>")]
		[Description("Path to the package file to publish")]
		public required string File { get; init; }
		
		[CommandArgument(1, "<community>")]
		[Description("Community where to publish the package")]
		public required string Community { get; init; }
		
		[CommandOption("--token", true)]
		[Description("Authentication token used to publish the package")]
		public required string Token { get; init; }
		
		[CommandOption("--repository")]
		[Description("URL of the server hosting the package")]
		[DefaultValue("https://thunderstore.io")]
		public string? Repository { get; init; }
		
		[CommandOption("--categories <VALUES>")]
		[Description("Categories used to label this package")]
		public string[]? Categories { get; init; }

		/// <inheritdoc />
		public override ValidationResult Validate()
		{
			if (!System.IO.File.Exists(File))
				return ValidationResult.Error($"No file was found at '{File}'.");
		
			if (string.IsNullOrWhiteSpace(Community))
				return ValidationResult.Error("Community cannot be empty.");
		
			if (string.IsNullOrWhiteSpace(Token))
				return ValidationResult.Error("Token cannot be empty.");
		
			if (!Uri.TryCreate(Repository, UriKind.Absolute, out var hostUri) || hostUri.Scheme != Uri.UriSchemeHttp && hostUri.Scheme != Uri.UriSchemeHttps)
				return ValidationResult.Error($"Repository '{Repository}' is not a valid URL.");

			if (Categories != null && Categories.Any(string.IsNullOrEmpty))
				return ValidationResult.Error("Categories contains an empty item.");

			return base.Validate();
		}
	}

	/// <inheritdoc />
	protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		// --- Settings ---
		var token = settings.Token;
		var file = settings.File;
		var community = settings.Community;
		var categories = settings.Categories ?? [];
		var repositoryUri = new Uri(settings.Repository!);
		// ---
		

		var builder = new RequestBuilder()
		              .ToHost(repositoryUri.Host)
		              .WithAuth(token);

		// api/experimental/
		
		// usermedia/initiate-upload/
		var uploadData = await ThunderstoreApi.InitiateMultipartUpload(
			file,
			builder,
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
			builder,
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
			builder,
			cancellationToken
		);

		return 0;
	}
}