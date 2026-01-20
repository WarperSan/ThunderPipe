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
		};

	[Theory]
	[MemberData(nameof(IsNameValidData))]
	public void IsNameValid(string name, bool expected)
	{
		var actual = RegexHelper.IsNameValid(name);

		Assert.Equal(expected, actual);
	}
}
