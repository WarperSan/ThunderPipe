using System.ComponentModel;
using System.Text.RegularExpressions;
using ThunderPipe.Infrastructure.TypeConverters;

namespace ThunderPipe.Models.Internal;

/// <summary>
/// Represents a valid package dependency string
/// </summary>
[TypeConverter(typeof(PackageDependencyTypeConverter))]
public sealed record PackageDependency
{
	private readonly string _dependencyString;
	private readonly string _namespace;
	private readonly PackageName _name;
	private readonly PackageVersion _version;

	public PackageDependency(string dependencyString)
	{
		_dependencyString = dependencyString;

		var components = _dependencyString.Split("-");

		_namespace = components.ElementAtOrDefault(0) ?? "";
		_name = new PackageName(components.ElementAtOrDefault(1) ?? "");
		_version = new PackageVersion(components.ElementAtOrDefault(2) ?? "");
	}

	/// <summary>
	/// Checks if the package dependency string is valid
	/// </summary>
	public bool IsValid()
	{
		return Regex.IsMatch(_namespace, "^(?!_)[a-zA-Z0-9_]+(?<!_)$")
			&& _name.IsValid()
			&& _version.IsValid();
	}

	/// <inheritdoc/>
	public override string ToString() => _dependencyString;

	public static implicit operator string(PackageDependency p) => p.ToString();
}
