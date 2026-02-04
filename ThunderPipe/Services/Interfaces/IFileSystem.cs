namespace ThunderPipe.Services.Interfaces;

/// <summary>
/// Interface representing any class that handles IO operations
/// </summary>
internal interface IFileSystem
{
	/// <summary>
	/// Opens an existing file for reading
	/// </summary>
	public FileStream OpenRead(string path);

	/// <summary>
	/// Asynchronously opens a binary file, reads the contents of the file into a byte array, and then closes the file
	/// </summary>
	public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken);

	/// <summary>
	/// Asynchronously creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten
	/// </summary>
	public Task WriteAllTextAsync(
		string path,
		string? contents,
		CancellationToken cancellationToken
	);

	/// <summary>
	/// Gets the name of the file
	/// </summary>
	public string GetName(string path);

	/// <summary>
	/// Gets the size in bytes of the file
	/// </summary>
	public long GetSize(string path);
}
