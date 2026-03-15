using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

[TypeConverter(typeof(StringCastTypeConverter<PackageName>))]
[JsonConverter(typeof(StringCastJsonConverter<PackageName>))]
public sealed record PackageName
{
	private readonly string _name;

	public PackageName(string name)
	{
		_name = name;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => Regex.IsMatch(_name, "^[a-zA-Z0-9_]+$");

	/// <inheritdoc/>
	public override string ToString() => _name;

	public static implicit operator string(PackageName p) => p.ToString();

	public static implicit operator PackageName(string name) => new(name);
}
