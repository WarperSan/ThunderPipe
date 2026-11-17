using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.Tests.Data;

/// <summary>
/// Test data for <see cref="PackageName"/> tests
/// </summary>
public class PackageNameTestData
{
	/// <summary>
	/// List of valid package names
	/// </summary>
	public static TheoryData<string> ValidNames =>
		["Belmont", "Starstorm2", "Cyto_Commando", "ExponentialItemStacks", "Some_Mod"];

	/// <summary>
	/// List of invalid package names
	/// </summary>
	public static TheoryData<string> InvalidNames =>
		["Some Mod", "HAND_OVERCLOCKED!", "Supply-Drop", ""];
}
