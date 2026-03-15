using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Models.Web.MultipartUpload;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Services.Implementations;

public sealed class PublicationService : IPublicationService
{
	private readonly PublishApiClient _client;
	private readonly ILogger _logger;
	private readonly IFileSystem _fileSystem;

	public PublicationService(RequestBuilder builder, IFileSystem fileSystem, ILogger logger)
	{
		_client = new PublishApiClient();
		_client.Builder = builder;
		_client.Logger = logger;

		_logger = logger;
		_fileSystem = fileSystem;
	}

	/// <inheritdoc />
	public async Task<Package> PublishPackage(
		string file,
		Team team,
		IEnumerable<Community> communities,
		IDictionary<Community, IEnumerable<Category>> categories,
		bool hasNsfw,
		string token,
		CancellationToken cancellationToken
	)
	{
		var uploadSession = await _client.InitiateMultipartUpload(
			file,
			_fileSystem,
			token,
			cancellationToken
		);

		_logger.LogInformation(
			"Uploading '{File}' ({GetSizeString}) in {ChunkCount} chunks.",
			file,
			GetSizeString(_fileSystem.GetSize(file)),
			uploadSession.Parts.Count
		);

		IReadOnlyCollection<UploadPart> uploadedParts;

		try
		{
			uploadedParts = await _client.UploadParts(
				file,
				uploadSession.Parts,
				_fileSystem,
				cancellationToken
			);
		}
		catch (Exception)
		{
			await _client.AbortMultipartUpload(uploadSession.UUID, token, cancellationToken);
			throw;
		}

		if (uploadedParts.Count != uploadSession.Parts.Count)
		{
			await _client.AbortMultipartUpload(uploadSession.UUID, token, cancellationToken);
			throw new InvalidOperationException("Failed to upload every parts.");
		}

		var finishedUpload = await _client.FinishMultipartUpload(
			uploadSession.UUID,
			uploadedParts,
			cancellationToken
		);

		if (!finishedUpload)
			throw new InvalidOperationException("Failed to finish upload.");

		_logger.LogInformation("Successfully finalized the upload.");

		return await _client.SubmitPackage(
			team,
			communities,
			categories,
			hasNsfw,
			uploadSession.UUID,
			cancellationToken
		);
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
