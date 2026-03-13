namespace ThunderPipe.Core.Models.Web.MultipartUpload;

public sealed record UploadPart
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
