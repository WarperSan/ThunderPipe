using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

public class PackageVersionTests
{
	[Theory]
	[InlineData(5, 4, 2121)]
	[InlineData(1, 0, 0)]
	[InlineData(1, 10, 0)]
	[InlineData(111, 0, 0)]
	[InlineData(0, 0, 110)]
	public void IsValid_WhenValidFromCtor_ReturnTrue(int major, int minor, int patch)
	{
		var packageVersion = new PackageVersion(major, minor, patch);
		Assert.True(packageVersion.IsValid());
	}

	[Theory]
	[InlineData(-1, 0, 0)]
	[InlineData(1, -10, 0)]
	[InlineData(1, 0, -100)]
	public void IsValid_WhenInvalidFromCtor_ReturnFalse(int major, int minor, int patch)
	{
		var packageVersion = new PackageVersion(major, minor, patch);
		Assert.False(packageVersion.IsValid());
	}

	[Theory]
	[InlineData("")]
	[InlineData("1")]
	[InlineData("1.0")]
	[InlineData("1.0-beta")]
	public void TryParse_WhenTooFewParts_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[InlineData("1.0.0.0")]
	[InlineData("1.0.0-beta")]
	[InlineData("12.0.0-alpha")]
	public void TryParse_WhenTooManyParts_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[InlineData("1a.5.5")]
	[InlineData("e.5.5")]
	public void TryParse_WhenInvalidMajor_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[InlineData("1.5a.5")]
	[InlineData("1.e.5")]
	public void TryParse_WhenInvalidMinor_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[InlineData("1.5.5a")]
	[InlineData("1.5.e")]
	public void TryParse_WhenInvalidPatch_ReturnFalse(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.False(success);
		Assert.Null(packageVersion);
	}

	[Theory]
	[InlineData("5.4.2121")]
	[InlineData("1.0.0")]
	[InlineData("1.10.0")]
	[InlineData("111.0.0")]
	public void TryParse_WhenValid_ReturnValid(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.True(success);
		Assert.NotNull(packageVersion);
		Assert.True(packageVersion.IsValid());
	}

	[Theory]
	[InlineData("-1.0.0")]
	[InlineData("1.-124678.0")]
	[InlineData("1.0.-621")]
	public void TryParse_WhenInvalid_ReturnInvalid(string version)
	{
		var success = PackageVersion.TryParse(version, out var packageVersion);

		Assert.True(success);
		Assert.NotNull(packageVersion);
		Assert.False(packageVersion.IsValid());
	}

	[Theory]
	[InlineData("-1.0.0")]
	[InlineData("1.-124678.0")]
	[InlineData("1.0.-621")]
	public void implicit_WhenInvalid_ReturnInvalid(string version)
	{
		PackageVersion packageVersion = version;

		Assert.NotNull(packageVersion);
		Assert.False(packageVersion.IsValid());
	}
}
