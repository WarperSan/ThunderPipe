using System.ComponentModel;
using System.Text.RegularExpressions;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

[TypeConverter(typeof(StringCastTypeConverter<Team>))]
public sealed record Team
{
	private readonly string _team;

	public Team(string team)
	{
		_team = team;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => Regex.IsMatch(_team, "^(?!_)[a-zA-Z0-9_]+(?<!_)$");

	public static implicit operator string(Team p) => p._team;

	public static implicit operator Team(string team) => new(team);
}
