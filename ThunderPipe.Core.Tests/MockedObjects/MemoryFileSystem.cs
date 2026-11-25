using System.Text;
using ThunderPipe.Core.Services.Interfaces;

namespace ThunderPipe.Core.Tests.MockedObjects;

/// <summary>
/// Implementation of <see cref="IFileSystem"/> that stores files into memory only
/// </summary>
public sealed class MemoryFileSystem : IFileSystem
{
	private readonly Dictionary<string, byte[]> _files = new();

	#region IFileSystem

	/// <inheritdoc />
	public Stream OpenRead(string path) => throw new FileNotFoundException();

	/// <inheritdoc />
	public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
	{
		if (!_files.TryGetValue(path, out var content))
			throw new FileNotFoundException(path);

		if (content == null)
			throw new FileLoadException();

		return Task.FromResult(content);
	}

	/// <inheritdoc />
	public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken)
	{
		var rawContent = Encoding.Default.GetBytes(content);
		_files[path] = rawContent;

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public string GetName(string path) => throw new FileNotFoundException();

	/// <inheritdoc />
	public long GetSize(string path) => throw new FileNotFoundException();

	#endregion
}
