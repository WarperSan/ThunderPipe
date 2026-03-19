using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents a package's version
/// </summary>
[TypeConverter(typeof(StringCastTypeConverter<PackageVersion>))]
[JsonConverter(typeof(StringCastJsonConverter<PackageVersion>))]
public sealed record PackageVersion
{
	private readonly string _version;

	public PackageVersion(string version)
	{
		_version = version;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => Regex.IsMatch(_version, "^[0-9]+.[0-9]+.[0-9]+$");

	public static implicit operator string(PackageVersion p) => p._version;

	public static implicit operator PackageVersion(string version) => new(version);
}
