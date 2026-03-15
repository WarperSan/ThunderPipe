namespace ThunderPipe.Core.Models.API;

public sealed record Package
{
	public PackageName Name { get; init; }
	public PackageVersion Version { get; init; }
	public Uri DownloadURL { get; init; }

	internal Package(string name, string version, string downloadUrl)
	{
		Name = name;
		Version = version;
		DownloadURL = new Uri(downloadUrl);
	}
}
