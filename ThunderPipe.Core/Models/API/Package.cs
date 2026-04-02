namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents a released package
/// </summary>
/// <remarks>
/// Only this library can create instances of this class
/// </remarks>
public sealed record Package
{
	public PackageName Name { get; }
	public PackageVersion Version { get; }
	public Uri DownloadURL { get; }

	internal Package(string name, string version, string downloadUrl)
	{
		Name = name;
		Version = version;
		DownloadURL = new Uri(downloadUrl);
	}
}
