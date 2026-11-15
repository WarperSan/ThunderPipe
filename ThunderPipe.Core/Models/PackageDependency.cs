using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
	/// Attempts to parse the given dependency string into a <see cref="PackageDependency"/>
	/// </summary>
	/// <remarks>
	/// Correctly parsing a <see cref="PackageDependency"/> does not guarantee it passes <see cref="IsValid"/>
	/// </remarks>
	[SuppressMessage("ReSharper", "DuplicatedSequentialIfBodies")]
	public static bool TryParse(
		string dependencyString,
		[NotNullWhen(true)] out PackageDependency? packageDependency
	)
	{
		var components = dependencyString.Split('-');

		if (components.Length != 3)
		{
			packageDependency = null;
			return false;
		}

		var rawTeam = components[0];
		var rawName = components[1];
		var rawVersion = components[2];

		if (rawTeam.Length == 0)
		{
			packageDependency = null;
			return false;
		}

		if (rawName.Length == 0)
		{
			packageDependency = null;
			return false;
		}

		if (rawVersion.Length == 0)
		{
			packageDependency = null;
			return false;
		}

		packageDependency = new PackageDependency(rawTeam, rawName, rawVersion);
		return true;
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

	public static implicit operator PackageDependency(string dependency)
	{
		if (TryParse(dependency, out var packageDependency))
			return packageDependency;

		return new PackageDependency("???", "???", "???");
	}
}
