using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ThunderPipe.Tests.Helpers;

/// <summary>
/// Class holding useful methods related to images
/// </summary>
internal static class ImageHelper
{
	/// <summary>
	/// Creates a blank 2D image with the given dimensions
	/// </summary>
	public static async Task<byte[]> CreateImage(int width, int height)
	{
		using Image<Rgba32> image = new(width, height);
		using var stream = new MemoryStream();

		await image.SaveAsPngAsync(stream);

		return stream.ToArray();
	}
}
