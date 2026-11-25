using ThunderPipe.Core.Models;
using ThunderPipe.Core.Tests.Data;
using ThunderPipe.Core.Tests.Infrastructure;

namespace ThunderPipe.Core.Tests.UnitTests.Models;

[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_MODEL)]
public class PackageNameTests
{
	[Theory]
	[MemberData(nameof(PackageNameTestData.ValidNames), MemberType = typeof(PackageNameTestData))]
	public void IsValid_WhenValid_ReturnTrue(string name)
	{
		var packageName = new PackageName(name);
		Assert.True(packageName.IsValid());
	}

	[Theory]
	[MemberData(nameof(PackageNameTestData.InvalidNames), MemberType = typeof(PackageNameTestData))]
	public void IsValid_WhenInvalid_ReturnFalse(string name)
	{
		var packageName = new PackageName(name);
		Assert.False(packageName.IsValid());
	}
}
