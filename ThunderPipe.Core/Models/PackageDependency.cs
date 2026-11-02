using System.ComponentModel;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models;

/// <summary>
/// Object that represents a package's dependency string
/// </summary>
[TypeConverter(typeof(StringCastTypeConverter<PackageDependency>))]
[JsonConverter(typeof(StringCastJsonConverter<PackageDependency>))]
public sealed record PackageDependency
{
	public PackageDependency(Team team, PackageName name, PackageVersion version)
	{
		Team = team;
		Name = name;
		Version = version;
	}

	/// <summary>
	/// Creates a <see cref="PackageDependency"/> from the given dependency string
	/// </summary>
	public static PackageDependency FromDependencyString(string dependencyString)
	{
		var components = dependencyString.Split("-", 3, StringSplitOptions.RemoveEmptyEntries);

		Team team;
		PackageName name;
		PackageVersion version;

		if (components.Length == 3)
		{
			team = components[0];
			name = components[1];
			version = components[2];
		}
		else
		{
			team = string.Empty;
			name = string.Empty;
			version = string.Empty;
		}

		return new PackageDependency(team, name, version);
	}

	public Team Team { get; }
	public PackageName Name { get; }
	public PackageVersion Version { get; }

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => Team.IsValid() && Name.IsValid() && Version.IsValid();

	/// <inheritdoc />
	public override string ToString() => $"{Team}-{Name}-{Version}";

	public static implicit operator string(PackageDependency p) => p.ToString();

	public static implicit operator PackageDependency(string dependency) =>
		FromDependencyString(dependency);
}
