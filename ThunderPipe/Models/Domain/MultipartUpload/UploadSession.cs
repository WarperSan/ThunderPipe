namespace ThunderPipe.Models.Domain.MultipartUpload;

internal sealed record UploadSession
{
	/// <summary>
	/// Identifier of the upload session
	/// </summary>
	public required string UUID { get; init; }

	/// <summary>
	/// Collection of all the parts to upload
	/// </summary>
	public required IReadOnlyCollection<UploadPartDescriptor> Parts { get; init; }
}
