using System.Text.RegularExpressions;
using ThunderPipe.Utils;

namespace ThunderPipe.Models.Internal;

/// <summary>
/// Represents a valid package name
/// </summary>
public sealed record PackageName
{
	private readonly string _name;

	public PackageName(string name)
	{
		_name = name;
	}

	/// <summary>
	/// Checks if the package name is valid
	/// </summary>
	public bool IsValid() => Regex.IsMatch("^[a-zA-Z0-9_]+$", _name);

	/// <inheritdoc/>
	public override string ToString() => _name;

	public static implicit operator string(PackageName p) => p.ToString();
}
