using ThunderPipe.Core.Models;
using ThunderPipe.Core.Tests.Data;
using ThunderPipe.Core.Tests.Infrastructure;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_MODEL)]
public class PackageDependencyTests
{
	[Theory]
	[MemberData(
		nameof(PackageDependencyTestData.ValidDependencies),
		MemberType = typeof(PackageDependencyTestData)
	)]
	public void IsValid_WhenValid_ReturnTrue(Team team, PackageName name, PackageVersion version)
	{
		var packageDependency = new PackageDependency(team, name, version);

		Assert.True(packageDependency.IsValid());
	}

	[Theory]
	[MemberData(
		nameof(PackageDependencyTestData.InvalidDependencies),
		MemberType = typeof(PackageDependencyTestData)
	)]
	public void IsValid_WhenInvalid_ReturnFalse(Team team, PackageName name, PackageVersion version)
	{
		var packageDependency = new PackageDependency(team, name, version);
		Assert.False(packageDependency.IsValid());
	}

	[Theory]
	[MemberData(
		nameof(PackageDependencyTestData.TooFewParts),
		MemberType = typeof(PackageDependencyTestData)
	)]
	public void TryParse_WhenTooFewParts_ReturnFalse(string dependencyString)
	{
		var success = PackageDependency.TryParse(dependencyString, out var packageDependency);

		Assert.False(success);
		Assert.Null(packageDependency);
	}

	[Theory]
	[MemberData(
		nameof(PackageDependencyTestData.TooManyParts),
		MemberType = typeof(PackageDependencyTestData)
	)]
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
