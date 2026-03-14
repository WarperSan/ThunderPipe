using System.ComponentModel;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

[TypeConverter(typeof(PackageDependencyTypeConverter))]
[JsonConverter(typeof(PackageTypeJsonConverter))]
public sealed record PackageDependency
{
	private readonly string _dependencyString;

	public PackageDependency(string dependencyString)
	{
		_dependencyString = dependencyString;

		var components = _dependencyString.Split("-", StringSplitOptions.RemoveEmptyEntries);

		if (components.Length == 3)
		{
			Team = new Team(components[0]);
			Name = components[1];
			Version = components[2];
		}
		else
		{
			Team = null;
			Name = null;
			Version = null;
		}
	}

	public Team? Team { get; }
	public PackageName? Name { get; }
	public PackageVersion? Version { get; }

	/// <summary>
	/// Checks if the underlying value is valid
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

	public static implicit operator PackageDependency(string dependency) => new(dependency);
}
