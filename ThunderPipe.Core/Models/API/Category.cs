using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

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
	public bool IsValid() => true;

	/// <inheritdoc/>
	public override string ToString() => _category;

	public static implicit operator string(Category p) => p.ToString();

	public static implicit operator Category(string category) => new(category);
}
