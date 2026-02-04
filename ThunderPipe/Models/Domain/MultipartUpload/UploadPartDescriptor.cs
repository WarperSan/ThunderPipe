namespace ThunderPipe.Models.Domain.MultipartUpload;

internal sealed record UploadPartDescriptor
{
	/// <summary>
	/// Identifier of the part
	/// </summary>
	public required int Id { get; init; }

	/// <summary>
	/// URL to upload the part to
	/// </summary>
	public required Uri UploadURL { get; init; }

	/// <summary>
	/// Offset in bytes of the part from the file start
	/// </summary>
	public required long Offset { get; init; }

	/// <summary>
	/// Size in bytes of the part
	/// </summary>
	public required int Size { get; init; }
}
