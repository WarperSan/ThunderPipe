using System.Text;
using ThunderPipe.Services.Interfaces;

namespace ThunderPipe.Tests.MockedObjects;

/// <summary>
/// Version of <see cref="IFileSystem"/> used for tests
/// </summary>
public class TestFileSystem : IFileSystem
{
	#region IFileSystem

	/// <inheritdoc />
	public FileStream OpenRead(string path) => throw new NotImplementedException();

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
	public Task WriteAllTextAsync(
		string path,
		string? contents,
		CancellationToken cancellationToken
	) => throw new NotImplementedException();

	/// <inheritdoc />
	public string GetName(string path) => throw new NotImplementedException();

	/// <inheritdoc />
	public long GetSize(string path) => throw new NotImplementedException();

	#endregion

	private readonly Dictionary<string, byte[]> _files = new();

	/// <summary>
	/// Sets the raw content to the given file
	/// </summary>
	public void SetContent(string path, byte[] content)
	{
		_files[path] = content;
	}

	/// <summary>
	/// Sets the text content to the given file
	/// </summary>
	public void SetContent(string path, string content)
	{
		var rawContent = Encoding.Default.GetBytes(content);
		SetContent(path, rawContent);
	}
}
