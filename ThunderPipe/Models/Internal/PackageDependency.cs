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

	public PackageDependency(string dependencyString)
	{
		_dependencyString = dependencyString;

		var components = _dependencyString.Split("-", StringSplitOptions.RemoveEmptyEntries);

		if (components.Length == 3)
		{
			Namespace = components[0];
			Name = new PackageName(components[1]);
			Version = new PackageVersion(components[2]);
		}
		else
		{
			Namespace = null;
			Name = null;
			Version = null;
		}
	}

	public string? Namespace { get; }
	public PackageName? Name { get; }
	public PackageVersion? Version { get; }

	/// <summary>
	/// Checks if the package dependency string is valid
	/// </summary>
	public bool IsValid()
	{
		if (Namespace == null)
			return false;

		if (Name == null)
			return false;

		if (Version == null)
			return false;

		return Regex.IsMatch(Namespace, "^(?!_)[a-zA-Z0-9_]+(?<!_)$")
			&& Name.IsValid()
			&& Version.IsValid();
	}

	/// <inheritdoc/>
	public override string ToString() => _dependencyString;

	public static implicit operator string(PackageDependency p) => p.ToString();
}
