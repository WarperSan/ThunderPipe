namespace ThunderPipe.Models.Domain.Submission;

internal sealed record PackageRelease
{
	/// <summary>
	/// Display name of the package
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Semantic version of the package
	/// </summary>
	public required Version Version { get; init; }

	/// <summary>
	/// URL to download the package
	/// </summary>
	public required Uri DownloadURL { get; init; }
}
