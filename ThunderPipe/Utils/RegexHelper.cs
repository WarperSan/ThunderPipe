using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ThunderPipe.Utils;

/// <summary>
/// Class that holds useful methods to use <see cref="Regex"/>
/// </summary>
[SuppressMessage("Performance", "SYSLIB1045:Convert to \'GeneratedRegexAttribute\'.")]
internal static class RegexHelper
{
	private const string REGEX_NAMESPACE = "(?!_)[a-zA-Z0-9_]+(?<!_)";
	private const string REGEX_NAME = "[a-zA-Z 0-9_]+";
	private const string REGEX_VERSION = "[0-9]+.[0-9]+.[0-9]+";

	/// <summary>
	/// Checks if the given name matches allowed names
	/// </summary>
	public static bool IsNameValid(string name) => Regex.IsMatch(name, $"^{REGEX_NAME}$");

	/// <summary>
	/// Checks if the given version matches allowed versions
	/// </summary>
	public static bool IsVersionValid(string version) =>
		Regex.IsMatch(version, $"^{REGEX_VERSION}$");

	/// <summary>
	/// Splits the given dependency string into its main components
	/// </summary>
	public static void SplitDependency(
		string dependencyString,
		out string? @namespace,
		out string? name,
		out string? version
	)
	{
		@namespace = null;
		name = null;
		version = null;

		var regex = new Regex(
			$"^(?<namespace>{REGEX_NAMESPACE})-(?<name>{REGEX_NAME})-(?<version>{REGEX_VERSION})$"
		);

		var match = regex.Match(dependencyString);

		if (match.Groups.TryGetValue("namespace", out var namespaceGroup))
			@namespace = namespaceGroup.Value;

		if (match.Groups.TryGetValue("name", out var nameGroup))
			name = nameGroup.Value;

		if (match.Groups.TryGetValue("version", out var versionGroup))
			version = versionGroup.Value;
	}
}
