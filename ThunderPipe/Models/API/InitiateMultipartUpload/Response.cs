using Newtonsoft.Json;

namespace ThunderPipe.Models.API.InitiateMultipartUpload;

internal record Response
{
	/// <summary>
	/// Model used to represent metadata from <see cref="PublishApiClient.InitiateMultipartUpload"/>
	/// </summary>
	public record FileMetadataModel
	{
		/// <summary>
		/// Unique identifier of the file
		/// </summary>
		[JsonProperty("uuid")]
		[JsonRequired]
		public required string UUID { get; set; }
	}

	/// <summary>
	/// Model used to represent multipart upload data from <see cref="PublishApiClient.InitiateMultipartUpload"/>
	/// </summary>
	/// <remarks>
	///	This refers to <a href="https://docs.aws.amazon.com/AmazonS3/latest/API/API_UploadPart.html">this</a>
	/// </remarks>
	public record UploadPartModel
	{
		/// <summary>
		/// Identifier of this part
		/// </summary>
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
