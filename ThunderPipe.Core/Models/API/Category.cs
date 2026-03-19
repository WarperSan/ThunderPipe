using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents an instance of a Thunderstore category
/// </summary>
[JsonConverter(typeof(StringCastJsonConverter<Category>))]
public sealed partial record Category
{
	private readonly string _category;

	public Category(string category)
	{
		_category = category;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => CategoryRegex().IsMatch(_category);

	public static implicit operator string(Category p) => p._category;

	public static implicit operator Category(string category) => new(category);

	[GeneratedRegex("^[a-z-A-Z\\-]+$")]
	private static partial Regex CategoryRegex();
}
