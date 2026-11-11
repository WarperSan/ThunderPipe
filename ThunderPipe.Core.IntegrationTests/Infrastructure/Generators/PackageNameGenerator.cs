using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>Generates a package name missing from the test service.</summary>
public static class PackageNameGenerator
{
    public static PackageName GenerateMissing()
    {
        return $"Missing_Package_{Guid.NewGuid():N}";
    }
}
