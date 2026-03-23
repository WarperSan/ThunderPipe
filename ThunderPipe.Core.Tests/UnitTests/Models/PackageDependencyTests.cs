using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

public class PackageDependencyTests
{
	[Theory]
	[InlineData("tristanmcpherson-R2API-5.0.5")]
	[InlineData("rob-Belmont-1.0.8")]
	[InlineData("Rune580-Risk_Of_Options-2.8.5")]
	public void IsValid_WhenValid_ReturnTrue(string dependencyString)
	{
		var packageDependency = new PackageDependency(dependencyString);
		Assert.True(packageDependency.IsValid());
	}

	[Theory]
	[InlineData("MrKixcat-Altered-Moons-2.0.1")]
	[InlineData("rob-Belmont-uwu-2.0.1")]
	public void IsValid_WhenInvalid_ReturnFalse(string dependencyString)
	{
		var packageDependency = new PackageDependency(dependencyString);
		Assert.False(packageDependency.IsValid());
	}

	[Theory]
	[InlineData("MrKixcat-AlteredMoons-2.0.1", "MrKixcat", "AlteredMoons", "2.0.1")]
	[InlineData("notnotnotswipez-MoreCompany-1.12.0", "notnotnotswipez", "MoreCompany", "1.12.0")]
	public void ctor_WhenIsComplete_ReturnParts(
		string dependencyString,
		string @namespace,
		string name,
		string version
	)
	{
		var packageDependency = new PackageDependency(dependencyString);

		Assert.True(packageDependency.IsValid());
		Assert.Equal(@namespace, packageDependency.Team);
		Assert.Equal(name, packageDependency.Name);
		Assert.Equal(version, packageDependency.Version);
	}

	[Theory]
	[InlineData("")]
	[InlineData("RugbugRedfern--5.0.0")]
	[InlineData("-ReservedItemSlotCore-")]
	[InlineData("sunnobunnoYippeeMod1.2.4")]
	public void ctor_WhenIsInvalid_ReturnEmpties(string dependencyString)
	{
		var packageDependency = new PackageDependency(dependencyString);

		Assert.False(packageDependency.IsValid());
		Assert.Equal(string.Empty, packageDependency.Team);
		Assert.Equal(string.Empty, packageDependency.Name);
		Assert.Equal(string.Empty, packageDependency.Version);
	}
}
