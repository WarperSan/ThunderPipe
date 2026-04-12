using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ThunderPipe.Core.Converters;

namespace ThunderPipe.Core.Models.API;

/// <summary>
/// Object that represents a package's name
/// </summary>
[TypeConverter(typeof(StringCastTypeConverter<PackageName>))]
[JsonConverter(typeof(StringCastJsonConverter<PackageName>))]
public sealed partial record PackageName
{
	private readonly string _name;

	public PackageName(string name)
	{
		_name = name;
	}

	/// <summary>
	/// Checks if the underlying value is valid
	/// </summary>
	public bool IsValid() => NameRegex().IsMatch(_name);

	/// <inheritdoc />
	public override string ToString() => _name;

	public static implicit operator string(PackageName p) => p.ToString();

	public static implicit operator PackageName(string name) => new(name);

#if NETSTANDARD
	static Regex? _nameRegex;

	private static Regex NameRegex() => _nameRegex ??= new Regex("^[a-zA-Z0-9_]+$");

#else
	[GeneratedRegex("^[a-zA-Z0-9_]+$")]
	private static partial Regex NameRegex();
#endif
}
