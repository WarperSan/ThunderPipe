using ThunderPipe.Services.Interfaces;

namespace ThunderPipe.Services.Implementations;

internal sealed class FileSystem : IFileSystem
{
	/// <inheritdoc />
	public FileStream OpenRead(string path) => File.OpenRead(path);

	/// <inheritdoc />
	public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken) =>
		File.ReadAllBytesAsync(path, cancellationToken);

	/// <inheritdoc />
	public Task WriteAllTextAsync(
		string path,
		string? contents,
		CancellationToken cancellationToken
	) => File.WriteAllTextAsync(path, contents, cancellationToken);

	/// <inheritdoc />
	public string GetName(string path) => Path.GetFileName(path);

	/// <inheritdoc/>
	public long GetSize(string path) => new FileInfo(path).Length;
}
