using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>Generates a team missing from the test service.</summary>
public static class TeamGenerator
{
    public static Team GenerateMissing()
    {
        return $"Missing_Team_{Guid.NewGuid():N}";
    }
}
