namespace ThunderPipe.Models.Domain.MultipartUpload;

internal sealed record UploadPart
{
	/// <summary>
	/// Identifier of the part
	/// </summary>
	public required int Id { get; init; }

	/// <summary>
	/// Identifier of the version
	/// </summary>
	public required string ETag { get; init; }
}
