using System.Text.RegularExpressions;

namespace ThunderPipe.Models.Internal;

/// <summary>
/// Represents a valid package version
/// </summary>
public sealed record PackageVersion
{
	private readonly string _version;

	public PackageVersion(string version)
	{
		_version = version;
	}

	/// <summary>
	/// Checks if the package version is valid
	/// </summary>
	public bool IsValid() => Regex.IsMatch(_version, "^[0-9]+.[0-9]+.[0-9]+$");

	/// <inheritdoc/>
	public override string ToString() => _version;

	public static implicit operator string(PackageVersion p) => p.ToString();
}
