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
	/// Attempts to parse the given version into a <see cref="PackageVersion"/>
	/// </summary>
	/// <remarks>
	/// Correctly parsing a <see cref="PackageVersion"/> does not guarantee it passes <see cref="IsValid"/>
	/// </remarks>
	[SuppressMessage("ReSharper", "DuplicatedSequentialIfBodies")]
	public static bool TryParse(
		string version,
		[NotNullWhen(true)] out PackageVersion? packageVersion
	)
	{
		var components = version.Split('.', 3);

		if (components.Length != 3)
		{
			packageVersion = null;
			return false;
		}

		if (!int.TryParse(components[0], out var major))
		{
			packageVersion = null;
			return false;
		}

		if (!int.TryParse(components[1], out var minor))
		{
			packageVersion = null;
			return false;
		}

		if (!int.TryParse(components[2], out var patch))
		{
			packageVersion = null;
			return false;
		}

		packageVersion = new PackageVersion(major, minor, patch);
		return true;
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

	public static implicit operator PackageVersion(string version)
	{
		if (TryParse(version, out var packageVersion))
			return packageVersion;

		return Invalid;
	}

	[GeneratedRegex("^[0-9]+.[0-9]+.[0-9]+$")]
	private static partial Regex VersionRegex();
}
