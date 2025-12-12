using Newtonsoft.Json;

namespace ThunderPipe.Models;

/// <summary>
/// Model used as the request payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.InitiateMultipartUpload"/>
/// </summary>
internal record InitiateUploadRequestModel
{
	/// <summary>
	/// Name of the file uploaded to the Amazon S3 bucket
	/// </summary>
	/// <remarks>
	/// It refers to <a href="http://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html">this</a>
	/// </remarks>
	[JsonProperty("filename")]
	[JsonRequired]
	public required string File { get; set; }

	/// <summary>
	/// Size of the file uploaded in bytes
	/// </summary>
	[JsonProperty("file_size_bytes")]
	[JsonRequired]
	public required long FileSize { get; set; }
}

/// <summary>
/// Model used as the response payload in <see cref="ThunderPipe.Utils.ThunderstoreApi.InitiateMultipartUpload"/>
/// </summary>
internal record InitiateUploadResponseModel
{
	/// <summary>
	/// Model used to represent metadata from <see cref="ThunderPipe.Utils.ThunderstoreApi.InitiateMultipartUpload"/>
	/// </summary>
	public record FileMetadataModel
	{
		/// <summary>
		/// Unique identifier of the user media
		/// </summary>
		[JsonProperty("uuid")]
		[JsonRequired]
		// ReSharper disable once InconsistentNaming
		public required string UUID { get; set; }

		// These fields are not used by this tool
		#if false
		[JsonProperty("filename")]
		public string? Filename { get; set; }

		[JsonProperty("size")]
		public long Size { get; set; }

		[JsonProperty("datetime_created")]
		public DateTime TimeCreated { get; set; }

		[JsonProperty("expiry")]
		public DateTime? ExpireTime { get; set; }

		[JsonProperty("status")]
		public string? Status { get; set; }
		#endif
	}

	/// <summary>
	/// Model used to represent multipart upload data from <see cref="ThunderPipe.Utils.ThunderstoreApi.InitiateMultipartUpload"/>
	/// </summary>
	/// <remarks>
	///	This refers to <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_UploadPart.html">this</a>
	/// </remarks>
	public record UploadPartModel
	{
		[JsonProperty("part_number")]
		[JsonRequired]
		public required int PartNumber { get; set; }

		/// <summary>
		/// URL to send the part to
		/// </summary>
		[JsonProperty("url")]
		[JsonRequired]
		public required string Url { get; set; }

		/// <summary>
		/// Offset of the part to send from the original file
		/// </summary>
		[JsonProperty("offset")]
		[JsonRequired]
		public required long Offset { get; set; }

		/// <summary>
		/// Size of the part to send
		/// </summary>
		[JsonProperty("length")]
		[JsonRequired]
		public required int Size { get; set; }
	}

	/// <summary>
	/// Metadata of the upload
	/// </summary>
	[JsonProperty("user_media")]
	[JsonRequired]
	public required FileMetadataModel FileMetadata { get; set; }

	/// <summary>
	/// Parts of the file to upload
	/// </summary>
	[JsonProperty("upload_urls")]
	[JsonRequired]
	public required UploadPartModel[] UploadParts { get; set; }
}