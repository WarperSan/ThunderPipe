using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.Tests.Data;

/// <summary>
/// Test data for <see cref="PackageDependency"/> tests
/// </summary>
public class PackageDependencyTestData
{
	/// <summary>
	/// List of valid dependency components
	/// </summary>
	public static TheoryData<Team, PackageName, PackageVersion> ValidDependencies =>
		new()
		{
			{ "tristanmcpherson", "R2API", "5.0.5" },
			{ "rob", "Belmont", "1.0.8" },
			{ "Rune580", "Risk_Of_Options", "2.8.5" },
			{ "Dadarabo", "MMMods", "1.0.0" },
			{ "BatoNagisa", "BepInExPack", "5.4.2102" },
		};

	/// <summary>
	/// List of invalid dependency components
	/// </summary>
	public static TheoryData<Team, PackageName, PackageVersion> InvalidDependencies =>
		new()
		{
			{ "???", "???", "???" },
			{ "JKLeckr?", "CupheadArchipelago", "0.2.100301101" },
			{ "MrKixcat", "Altered-Moons", "2.0.1" },
			{ "rob", "Belmont", "2.0.1#beta" },
		};

	/// <summary>
	/// List of dependency strings with fewer than <c>3</c> components
	/// </summary>
	public static TheoryData<string> TooFewParts =>
		["", "RugbugRedfern-5.0.0", "sunnobunnoYippeeMod1.2.4", "-", "team", "  "];

	/// <summary>
	/// List of dependency strings with more than <c>3</c> components
	/// </summary>
	public static TheoryData<string> TooManyParts =>
		[
			"???-???-???-???",
			"Codaaaaaa-RemoveSlapCooldown-1.0.0-beta",
			"Codaaaaaa-RemoveSlapCooldown-1.0.0-ThunderStore-beta",
			"a-b-c-d-e",
			"team-mod-1.0.0-rc.1",
			"x-y-z-w-v-u",
		];
}
