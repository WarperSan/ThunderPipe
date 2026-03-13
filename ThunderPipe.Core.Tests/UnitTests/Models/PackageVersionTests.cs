using ThunderPipe.Models.Internal;

namespace ThunderPipe.Tests.UnitTests.Models;

public class PackageVersionTests
{
	[Theory]
	[InlineData("5.4.2121")]
	[InlineData("1.0.0")]
	[InlineData("1.10.0")]
	[InlineData("111.0.0")]
	public void IsValid_WhenValid_ReturnTrue(string version)
	{
		var packageVersion = new PackageVersion(version);
		Assert.True(packageVersion.IsValid());
	}

	[Theory]
	[InlineData("")]
	[InlineData("1.0")]
	[InlineData("1.0.0.0")]
	[InlineData("1.0.0-beta")]
	[InlineData("12.0.0-alpha")]
	[InlineData("1.5.5e")]
	public void IsValid_WhenInvalid_ReturnFalse(string version)
	{
		var packageVersion = new PackageVersion(version);
		Assert.False(packageVersion.IsValid());
	}
}
