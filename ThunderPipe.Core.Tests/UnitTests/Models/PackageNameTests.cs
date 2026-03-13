using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Tests.UnitTests.Models;

public class PackageNameTests
{
	[Theory]
	[InlineData("Belmont")]
	[InlineData("Starstorm2")]
	[InlineData("Cyto_Commando")]
	[InlineData("ExponentialItemStacks")]
	[InlineData("Some_Mod")]
	public void IsValid_WhenValid_ReturnTrue(string name)
	{
		var packageName = new PackageName(name);
		Assert.True(packageName.IsValid());
	}

	[Theory]
	[InlineData("Some Mod")]
	[InlineData("HAND_OVERCLOCKED!")]
	[InlineData("Supply-Drop")]
	[InlineData("")]
	public void IsValid_WhenInvalid_ReturnFalse(string name)
	{
		var packageName = new PackageName(name);
		Assert.False(packageName.IsValid());
	}
}
