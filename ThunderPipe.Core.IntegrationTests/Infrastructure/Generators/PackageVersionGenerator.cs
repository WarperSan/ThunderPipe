using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>
/// Maintainer's greater-version generator, adapted to the original string model.
/// </summary>
public static class PackageVersionGenerator
{
    public static PackageVersion GenerateGreater(PackageVersion version)
    {
        string[] parts = version.ToString().Split('.');
        if (parts.Length != 3
            || !int.TryParse(parts[0], out int major)
            || !int.TryParse(parts[1], out int minor)
            || !int.TryParse(parts[2], out int patch))
            throw new ArgumentException("Expected a three-component version.", nameof(version));

        if (major < int.MaxValue)
            return new PackageVersion($"{major + 1}.{minor}.{patch}");
        if (minor < int.MaxValue)
            return new PackageVersion($"{major}.{minor + 1}.{patch}");
        if (patch < int.MaxValue)
            return new PackageVersion($"{major}.{minor}.{patch + 1}");
        throw new ArgumentException("Version is already at its maximum.", nameof(version));
    }
}
