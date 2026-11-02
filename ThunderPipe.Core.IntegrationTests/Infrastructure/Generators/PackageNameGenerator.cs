using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>
/// Class used to generate instances of <see cref="PackageName"/>
/// </summary>
public static class PackageNameGenerator
{
	/// <summary>
	/// Generates a <see cref="PackageName"/> that is missing from the service
	/// </summary>
	public static PackageName GenerateMissing()
	{
		return $"Missing_Package_{Guid.NewGuid():N}";
	}
}
