using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Utils;

public class RegexHelperTests
{
	public static IEnumerable<object[]> IsNameValidData =>
		new List<object[]>
		{
			new object[] { "Belmont", true },
			new object[] { "Starstorm2", true },
			new object[] { "Cyto_Commando", true },
			new object[] { "ExponentialItemStacks", true },
			new object[] { "Some_Mod", true },
			new object[] { "Some Mod", false },
			new object[] { "HAND_OVERCLOCKED!", false },
			new object[] { "Supply-Drop", false },
			new object[] { "", false },
		};

	[Theory]
	[MemberData(nameof(IsNameValidData))]
	public void IsNameValid(string name, bool expected)
	{
		var actual = RegexHelper.IsNameValid(name);

		Assert.Equal(expected, actual);
	}

	public static IEnumerable<object[]> IsVersionValidData =>
		new List<object[]>
		{
			new object[] { "5.4.2121", true },
			new object[] { "1.0.0", true },
			new object[] { "1.10.0", true },
			new object[] { "111.0.0", true },
			new object[] { "", false },
			new object[] { "1.0", false },
			new object[] { "1.0.0.0", false },
			new object[] { "1.0.0-beta", false },
			new object[] { "12.0.0-alpha", false },
			new object[] { "1.5.5e", false },
		};

	[Theory]
	[MemberData(nameof(IsVersionValidData))]
	public void IsVersionValid(string version, bool expected)
	{
		var actual = RegexHelper.IsVersionValid(version);

		Assert.Equal(expected, actual);
	}

	public static IEnumerable<object[]> IsDependencyValidData =>
		new List<object[]>
		{
			new object[] { "tristanmcpherson-R2API-5.0.5", true },
			new object[] { "rob-Belmont-1.0.8", true },
			new object[] { "Rune580-Risk_Of_Options-2.8.5", true },
			new object[] { "MrKixcat-Altered-Moons-2.0.1", false },
			new object[] { "rob-Belmont-uwu-2.0.1", false },
		};

	[Theory]
	[MemberData(nameof(IsDependencyValidData))]
	public void IsDependencyValid(string dependencyString, bool expected)
	{
		var actual = RegexHelper.IsDependencyValid(dependencyString);

		Assert.Equal(expected, actual);
	}

	public static IEnumerable<object?[]> SplitDependencyStringData =>
		new List<object?[]>
		{
			new object?[] { "MrKixcat-AlteredMoons-2.0.1", "MrKixcat", "AlteredMoons", "2.0.1" },
			new object?[]
			{
				"notnotnotswipez-MoreCompany-1.12.0",
				"notnotnotswipez",
				"MoreCompany",
				"1.12.0",
			},
			new object?[] { "RugbugRedfern--5.0.0", null, null, null },
			new object?[] { "-ReservedItemSlotCore-", null, null, null },
			new object?[] { "", null, null, null },
			new object?[] { "sunnobunnoYippeeMod1.2.4", null, null, null },
		};

	[Theory]
	[MemberData(nameof(SplitDependencyStringData))]
	public void SplitDependencyString(
		string dependencyString,
		string? @namespace,
		string? name,
		string? version
	)
	{
		RegexHelper.SplitDependency(
			dependencyString,
			out var actualNamespace,
			out var actualName,
			out var actualVersion
		);

		Assert.Equal(@namespace, actualNamespace);
		Assert.Equal(name, actualName);
		Assert.Equal(version, actualVersion);
	}
}
