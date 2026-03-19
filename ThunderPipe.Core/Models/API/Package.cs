namespace ThunderPipe.Core.Models.API;

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
