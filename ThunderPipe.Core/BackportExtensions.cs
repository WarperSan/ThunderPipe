#if NETSTANDARD
using System.Net.Mime;
using System.Text;

internal static class BackportExtensions
{
	public static Task WriteAsync(
		this MemoryStream stream,
		byte[] buffer,
		CancellationToken cancellationToken
	) => stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);

	extension(File)
	{
		public static async Task<byte[]> ReadAllBytesAsync(
			string path,
			CancellationToken cancellationToken
		)
		{
			using var stream = File.OpenRead(path);
			byte[] result = new byte[stream.Length];
			_ = await stream.ReadAsync(result, 0, (int)stream.Length, cancellationToken);
			return result;
		}

		public static Task WriteAllTextAsync(
			string path,
			string? contents,
			CancellationToken cancellationToken
		)
		{
			using var stream = File.OpenWrite(path);
			var bytes = Encoding.UTF8.GetBytes(contents);
			return stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
		}
	}
}
#endif
