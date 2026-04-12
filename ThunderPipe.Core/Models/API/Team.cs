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

	/// <inheritdoc />
	public override string ToString() => _team;

	public static implicit operator string(Team p) => p.ToString();

	public static implicit operator Team(string team) => new(team);

#if NETSTANDARD
	static Regex? _teamRegex;

	private static Regex TeamRegex() => _teamRegex ??= new Regex("^(?!_)[a-zA-Z0-9_]+(?<!_)$");
#else
	[GeneratedRegex("^(?!_)[a-zA-Z0-9_]+(?<!_)$")]
	private static partial Regex TeamRegex();
#endif
}
