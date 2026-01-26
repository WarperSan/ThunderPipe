using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Utils;

public class RegexHelperTests
{
	[Theory]
	[InlineData("Belmont")]
	[InlineData("Starstorm2")]
	[InlineData("Cyto_Commando")]
	[InlineData("ExponentialItemStacks")]
	[InlineData("Some_Mod")]
	public void IsNameValid_WhenValid_ReturnTrue(string name)
	{
		var actual = RegexHelper.IsNameValid(name);
		Assert.True(actual);
	}

	[Theory]
	[InlineData("Some Mod")]
	[InlineData("HAND_OVERCLOCKED!")]
	[InlineData("Supply-Drop")]
	[InlineData("")]
	public void IsNameValid_WhenInvalid_ReturnFalse(string name)
	{
		var actual = RegexHelper.IsNameValid(name);
		Assert.False(actual);
	}

	[Theory]
	[InlineData("5.4.2121")]
	[InlineData("1.0.0")]
	[InlineData("1.10.0")]
	[InlineData("111.0.0")]
	public void IsVersionValid_WhenValid_ReturnTrue(string version)
	{
		var actual = RegexHelper.IsVersionValid(version);
		Assert.True(actual);
	}

	[Theory]
	[InlineData("")]
	[InlineData("1.0")]
	[InlineData("1.0.0.0")]
	[InlineData("1.0.0-beta")]
	[InlineData("12.0.0-alpha")]
	[InlineData("1.5.5e")]
	public void IsVersionValid_WhenInvalid_ReturnFalse(string version)
	{
		var actual = RegexHelper.IsVersionValid(version);
		Assert.False(actual);
	}

	[Theory]
	[InlineData("tristanmcpherson-R2API-5.0.5")]
	[InlineData("rob-Belmont-1.0.8")]
	[InlineData("Rune580-Risk_Of_Options-2.8.5")]
	public void IsDependencyValid_WhenValid_ReturnTrue(string dependencyString)
	{
		var actual = RegexHelper.IsDependencyValid(dependencyString);
		Assert.True(actual);
	}

	[Theory]
	[InlineData("MrKixcat-Altered-Moons-2.0.1")]
	[InlineData("rob-Belmont-uwu-2.0.1")]
	public void IsDependencyValid_WhenInvalid_ReturnFalse(string dependencyString)
	{
		var actual = RegexHelper.IsDependencyValid(dependencyString);
		Assert.False(actual);
	}

	[Theory]
	[InlineData("MrKixcat-AlteredMoons-2.0.1", "MrKixcat", "AlteredMoons", "2.0.1")]
	[InlineData("notnotnotswipez-MoreCompany-1.12.0", "notnotnotswipez", "MoreCompany", "1.12.0")]
	public void SplitDependency_WhenIsComplete_ReturnParts(
		string dependencyString,
		string @namespace,
		string name,
		string version
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

	[Theory]
	[InlineData("")]
	[InlineData("RugbugRedfern--5.0.0")]
	[InlineData("-ReservedItemSlotCore-")]
	[InlineData("sunnobunnoYippeeMod1.2.4")]
	public void SplitDependency_WhenIsInvalid_ReturnNulls(string dependencyString)
	{
		RegexHelper.SplitDependency(
			dependencyString,
			out var actualNamespace,
			out var actualName,
			out var actualVersion
		);

		Assert.Null(actualNamespace);
		Assert.Null(actualName);
		Assert.Null(actualVersion);
	}
}
