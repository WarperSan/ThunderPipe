using System.ComponentModel;
using ThunderPipe.Core.TypeConverters;

namespace ThunderPipe.Core.Models.API;

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
			Team = new PackageTeam(components[0]);
			Name = new PackageName(components[1]);
			Version = new PackageVersion(components[2]);
		}
		else
		{
			Team = null;
			Name = null;
			Version = null;
		}
	}

	public PackageTeam? Team { get; }
	public PackageName? Name { get; }
	public PackageVersion? Version { get; }

	/// <summary>
	/// Checks if the package dependency string is valid
	/// </summary>
	public bool IsValid()
	{
		if (Team == null)
			return false;

		if (Name == null)
			return false;

		if (Version == null)
			return false;

		return Team.IsValid() && Name.IsValid() && Version.IsValid();
	}

	/// <inheritdoc/>
	public override string ToString() => _dependencyString;

	public static implicit operator string(PackageDependency p) => p.ToString();
}
