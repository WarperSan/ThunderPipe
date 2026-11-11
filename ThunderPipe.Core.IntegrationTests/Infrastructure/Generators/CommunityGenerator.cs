using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>Generates a community missing from the test service.</summary>
public static class CommunityGenerator
{
    public static Community GenerateMissing()
    {
        return $"missing-community-{Guid.NewGuid():N}";
    }
}
