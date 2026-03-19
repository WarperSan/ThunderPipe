using System.ComponentModel;
using System.Text.RegularExpressions;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents a Thunderstore team
/// </summary>
[TypeConverter(typeof(StringCastTypeConverter<Team>))]
public sealed partial record Team
{
	private readonly string _team;

	public Team(string team)
	{
		_team = team;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => TeamRegex().IsMatch(_team);

	public static implicit operator string(Team p) => p._team;

	public static implicit operator Team(string team) => new(team);

	[GeneratedRegex("^(?!_)[a-zA-Z0-9_]+(?<!_)$")]
	private static partial Regex TeamRegex();
}
