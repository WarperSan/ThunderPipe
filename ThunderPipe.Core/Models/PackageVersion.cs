using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models;

/// <summary>
/// Object that represents a package's version
/// </summary>
[TypeConverter(typeof(StringCastTypeConverter<PackageVersion>))]
[JsonConverter(typeof(StringCastJsonConverter<PackageVersion>))]
public sealed partial record PackageVersion
{
	private static readonly PackageVersion Invalid = new(0, 0, 0) { IsPlaceholder = true };

	/// <summary>
	/// Defines if the fields are placeholders or not
	/// </summary>
	private bool IsPlaceholder { get; init; }

	public PackageVersion(int major, int minor, int patch)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
		IsPlaceholder = false;
	}

	/// <summary>
	/// Creates a <see cref="PackageVersion"/> from the given version string
	/// </summary>
	[SuppressMessage("ReSharper", "DuplicatedSequentialIfBodies")]
	public static PackageVersion FromVersion(string version)
	{
		var components = version.Split(".", 3);

		if (components.Length != 3)
			return Invalid;

		if (!int.TryParse(components[0], out var major))
			return Invalid;

		if (!int.TryParse(components[1], out var minor))
			return Invalid;

		if (!int.TryParse(components[2], out var patch))
			return Invalid;

		return new PackageVersion(major, minor, patch);
	}

	public int Major { get; }
	public int Minor { get; }
	public int Patch { get; }

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid()
	{
		if (IsPlaceholder)
			return false;

		return VersionRegex().IsMatch(this);
	}

	/// <inheritdoc />
	public override string ToString() => $"{Major}.{Minor}.{Patch}";

	public static implicit operator string(PackageVersion p) => p.ToString();

	public static implicit operator PackageVersion(string version) => FromVersion(version);

	[GeneratedRegex("^[0-9]+.[0-9]+.[0-9]+$")]
	private static partial Regex VersionRegex();
}
