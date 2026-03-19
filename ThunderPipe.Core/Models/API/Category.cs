using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents an instance of a Thunderstore category
/// </summary>
[JsonConverter(typeof(StringCastJsonConverter<Category>))]
public sealed record Category
{
	private readonly string _category;

	public Category(string category)
	{
		_category = category;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	// ReSharper disable once MemberCanBeMadeStatic.Global
	public bool IsValid() => true;

	public static implicit operator string(Category p) => p._category;

	public static implicit operator Category(string category) => new(category);
}
