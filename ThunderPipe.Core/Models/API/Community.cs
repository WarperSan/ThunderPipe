using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents an instance of a Thunderstore community
/// </summary>
[JsonConverter(typeof(StringCastJsonConverter<Community>))]
public sealed partial record Community
{
	private readonly string _community;

	public Community(string community)
	{
		_community = community;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => CommunityRegex().IsMatch(_community);

	/// <inheritdoc />
	public override string ToString() => _community;

	public static implicit operator string(Community p) => p.ToString();

	public static implicit operator Community(string community) => new(community);

	[GeneratedRegex("^[a-zA-Z0-9-]+$")]
	private static partial Regex CommunityRegex();
}
