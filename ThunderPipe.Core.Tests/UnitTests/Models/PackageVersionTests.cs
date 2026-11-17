using ThunderPipe.Core.Models;
using ThunderPipe.Core.Tests.Data;
using ThunderPipe.Core.Tests.Infrastructure;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_MODEL)]
public class PackageVersionTests
{
	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.ValidVersions),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void IsValid_WhenValidFromCtor_ReturnTrue(int major, int minor, int patch)
	{
		var packageVersion = new PackageVersion(major, minor, patch);
		Assert.True(packageVersion.IsValid());
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.InvalidVersions),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void IsValid_WhenInvalidFromCtor_ReturnFalse(int major, int minor, int patch)
	{
		var packageVersion = new PackageVersion(major, minor, patch);
		Assert.False(packageVersion.IsValid());
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.TooFewParts),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenTooFewParts_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.TooManyParts),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenTooManyParts_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.InvalidMajor),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenInvalidMajor_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.InvalidMinor),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenInvalidMinor_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.InvalidPatch),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenInvalidPatch_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.ValidParseVersions),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenValid_ReturnValid(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.True(success);
		Assert.NotNull(packageVersion);
		Assert.True(packageVersion.IsValid());
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.InvalidParseVersions),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenInvalid_ReturnInvalid(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.True(success);
		Assert.NotNull(packageVersion);
		Assert.False(packageVersion.IsValid());
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.OutOfRangeVersions),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void TryParse_WhenOutOfRange_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[MemberData(
		nameof(PackageVersionTestData.InvalidVersionStrings),
		MemberType = typeof(PackageVersionTestData)
	)]
	public void implicit_WhenFailedParsing_ReturnInvalid(string version)
	{
		PackageVersion packageVersion = version;

		Assert.NotNull(packageVersion);
		Assert.False(packageVersion.IsValid());
	}
}
