using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

public class PackageVersionTests
{
	[Theory]
	[InlineData("5.4.2121")]
	[InlineData("1.0.0")]
	[InlineData("1.10.0")]
	[InlineData("111.0.0")]
	public void IsValid_WhenValid_ReturnTrue(string version)
	{
		var packageVersion = PackageVersion.FromVersion(version);
		Assert.True(packageVersion.IsValid());
	}

	[Theory]
	[InlineData("")]
	[InlineData("1-0-0")]
	[InlineData("1/0/0")]
	[InlineData("1..0.0")]
	[InlineData("1...0.0")]
	[InlineData("1.0")]
	[InlineData("1.0.0.0")]
	[InlineData("1.0.0-beta")]
	[InlineData("12.0.0-alpha")]
	[InlineData("1.5.5e")]
	[InlineData("1.5e.5")]
	[InlineData("1e.5.5")]
	[InlineData("1e.5e.5")]
	[InlineData("1.5e.5e")]
	[InlineData("1e.5e.5e")]
	[InlineData("1a5.5e")]
	public void IsValid_WhenInvalid_ReturnFalse(string version)
	{
		var packageVersion = PackageVersion.FromVersion(version);
		Assert.False(packageVersion.IsValid());
	}

	[Theory]
	[InlineData(0, 0, 0)]
	[InlineData(1, 0, 0)]
	[InlineData(1, 10, 0)]
	[InlineData(111, 0, 0)]
	[InlineData(1, 5, 5)]
	[InlineData(12, 0, 0)]
	[InlineData(5, 4, 2121)]
	public void IsValid_WhenManualValid_ReturnTrue(int major, int minor, int patch)
	{
		var version = new PackageVersion(major, minor, patch);

		Assert.True(version.IsValid());
	}

	[Theory]
	[InlineData(-1, 0, 0)]
	[InlineData(0, -20, 0)]
	[InlineData(0, 0, -200)]
	[InlineData(int.MinValue, 0, -200)]
	[InlineData(-1, -1, int.MaxValue)]
	[InlineData(-20, -200, int.MinValue)]
	public void IsValid_WhenManualInvalid_ReturnFalse(int major, int minor, int patch)
	{
		var version = new PackageVersion(major, minor, patch);

		Assert.False(version.IsValid());
	}
}
