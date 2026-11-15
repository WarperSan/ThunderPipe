using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

public class PackageDependencyTests
{
	[Theory]
	[InlineData("tristanmcpherson", "R2API", "5.0.5")]
	[InlineData("rob", "Belmont", "1.0.8")]
	[InlineData("Rune580", "Risk_Of_Options", "2.8.5")]
	public void IsValid_WhenValid_ReturnTrue(Team team, PackageName name, PackageVersion version)
	{
		var packageDependency = new PackageDependency(team, name, version);

		Assert.True(packageDependency.IsValid());
	}

	[Theory]
	[InlineData("???", "???", "???")]
	[InlineData("JKLeckr?", "CupheadArchipelago", "0.2.100301101")]
	[InlineData("MrKixcat", "Altered-Moons", "2.0.1")]
	[InlineData("rob", "Belmont", "2.0.1#beta")]
	public void IsValid_WhenInvalid_ReturnFalse(Team team, PackageName name, PackageVersion version)
	{
		var packageDependency = new PackageDependency(team, name, version);
		Assert.False(packageDependency.IsValid());
	}

	[Theory]
	[InlineData("")]
	[InlineData("RugbugRedfern-5.0.0")]
	[InlineData("sunnobunnoYippeeMod1.2.4")]
	public void TryParse_WhenTooFewParts_ReturnFalse(string dependencyString)
	{
		var success = PackageDependency.TryParse(dependencyString, out var packageDependency);

		Assert.False(success);
		Assert.Null(packageDependency);
	}

	[Theory]
	[InlineData("???-???-???-???")]
	[InlineData("Codaaaaaa-RemoveSlapCooldown-1.0.0-beta")]
	[InlineData("Codaaaaaa-RemoveSlapCooldown-1.0.0-ThunderStore-beta")]
	public void TryParse_WhenTooManyParts_ReturnFalse(string dependencyString)
	{
		var success = PackageDependency.TryParse(dependencyString, out var packageDependency);

		Assert.False(success);
		Assert.Null(packageDependency);
	}

	[Fact]
	public void TryParse_WhenEmptyTeam_ReturnFalse()
	{
		const string DEPENDENCY_STRING = "-GaleModManager-1.23.3";

		var success = PackageDependency.TryParse(DEPENDENCY_STRING, out var packageDependency);

		Assert.False(success);
		Assert.Null(packageDependency);
	}

	[Fact]
	public void TryParse_WhenEmptyName_ReturnFalse()
	{
		const string DEPENDENCY_STRING = "Kesomannen--1.23.3";

		var success = PackageDependency.TryParse(DEPENDENCY_STRING, out var packageDependency);

		Assert.False(success);
		Assert.Null(packageDependency);
	}

	[Fact]
	public void TryParse_WhenEmptyVersion_ReturnFalse()
	{
		const string DEPENDENCY_STRING = "Kesomannen-GaleModManager-";

		var success = PackageDependency.TryParse(DEPENDENCY_STRING, out var packageDependency);

		Assert.False(success);
		Assert.Null(packageDependency);
	}
}
