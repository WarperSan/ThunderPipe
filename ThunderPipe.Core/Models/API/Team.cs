using System.ComponentModel;
using System.Text.RegularExpressions;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Represents a valid team
/// </summary>
[TypeConverter(typeof(TeamTypeConverter))]
public sealed record Team
{
	private readonly string _team;

	public Team(string team)
	{
		_team = team;
	}

	/// <summary>
	/// Checks if the <see cref="Team"/> is valid
	/// </summary>
	public bool IsValid() => Regex.IsMatch(_team, "^(?!_)[a-zA-Z0-9_]+(?<!_)$");

	/// <inheritdoc/>
	public override string ToString() => _team;

	public static implicit operator string(Team p) => p.ToString();
}
