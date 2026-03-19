using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

[JsonConverter(typeof(StringCastJsonConverter<Community>))]
public sealed record Community
{
	private readonly string _community;

	public Community(string community)
	{
		_community = community;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public bool IsValid() => true;

	public static implicit operator string(Community p) => p._community;

	public static implicit operator Community(string community) => new(community);
}
