using System.ComponentModel;
using System.Text.RegularExpressions;
using ThunderPipe.Core.TypeConverters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Represents a valid package version
/// </summary>
[TypeConverter(typeof(PackageVersionTypeConverter))]
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
