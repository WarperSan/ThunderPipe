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

	public static implicit operator PackageDependency(string dependency) => new(dependency);
}
