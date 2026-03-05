using System.ComponentModel;
using System.Text.RegularExpressions;
using ThunderPipe.Infrastructure.TypeConverters;

namespace ThunderPipe.Models.Internal;

/// <summary>
/// Represents a valid package team
/// </summary>
[TypeConverter(typeof(PackageTeamTypeConverter))]
public sealed record PackageTeam
{
	private readonly string _team;

	public PackageTeam(string team)
	{
		_team = team;
	}

	/// <summary>
	/// Checks if the package team is valid
	/// </summary>
	public bool IsValid() => Regex.IsMatch(_team, "^(?!_)[a-zA-Z0-9_]+(?<!_)$");

	/// <inheritdoc/>
	public override string ToString() => _team;

	public static implicit operator string(PackageTeam p) => p.ToString();
}
