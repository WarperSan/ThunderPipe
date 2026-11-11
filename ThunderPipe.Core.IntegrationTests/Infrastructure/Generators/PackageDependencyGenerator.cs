using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>
/// Maintainer's dependency data generator, adapted to the original Core model.
/// #124 uses PackageDependency(Team, PackageName, PackageVersion), whereas this
/// branch still uses PackageDependency(string).
/// </summary>
public static class PackageDependencyGenerator
{
    public static PackageDependency ChangeTeam(PackageDependency dependency, Team team)
    {
        return new PackageDependency($"{team}-{dependency.Name}-{dependency.Version}");
    }

    public static PackageDependency ChangeName(PackageDependency dependency, PackageName name)
    {
        return new PackageDependency($"{dependency.Team}-{name}-{dependency.Version}");
    }

    public static PackageDependency ChangeVersion(PackageDependency dependency, PackageVersion version)
    {
        return new PackageDependency($"{dependency.Team}-{dependency.Name}-{version}");
    }
}
