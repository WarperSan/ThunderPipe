using System.ComponentModel;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents a package's dependency string
/// </summary>
[TypeConverter(typeof(StringCastTypeConverter<PackageDependency>))]
[JsonConverter(typeof(StringCastJsonConverter<PackageDependency>))]
public sealed record PackageDependency
{
	private readonly string _dependencyString;

	public PackageDependency(string dependencyString)
	{
		_dependencyString = dependencyString;

		var components = _dependencyString.Split("-", 3, StringSplitOptions.RemoveEmptyEntries);

		if (components.Length == 3)
		{
			Team = components[0];
			Name = components[1];
			Version = components[2];
		}
		else
		{
			Team = string.Empty;
			Name = string.Empty;
			Version = string.Empty;
		}
	}

	public Team Team { get; }
	public PackageName Name { get; }
	public PackageVersion Version { get; }

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => Team.IsValid() && Name.IsValid() && Version.IsValid();

	/// <inheritdoc />
	public override string ToString() => _dependencyString;

	public static implicit operator string(PackageDependency p) => p.ToString();

	public static implicit operator PackageDependency(string dependency) => new(dependency);
}
