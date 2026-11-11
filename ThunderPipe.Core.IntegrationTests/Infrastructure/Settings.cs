using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure;

/// <summary>
/// Maintainer's named test settings, adapted to the existing CI fixture JSON.
/// Optional THUNDERSTORE_* environment overrides remain supported.
/// </summary>
public static class Settings
{
    public static readonly Uri BaseUri = new(
        Environment.GetEnvironmentVariable("THUNDERSTORE_URL")
        ?? Environment.GetEnvironmentVariable("THUNDERPIPE_BASE_URL")
        ?? "http://thunderstore.localhost/",
        UriKind.Absolute);

    public static Community ExistingCommunity =>
        new(Environment.GetEnvironmentVariable("THUNDERSTORE_COMMUNITY")
            ?? ThunderstoreFixture.LoadFixture().Community);

    public static PackageDependency ExistingPackage
    {
        get
        {
            FixtureData data = ThunderstoreFixture.LoadFixture();
            return new PackageDependency(
                Environment.GetEnvironmentVariable("THUNDERSTORE_PACKAGE")
                ?? $"{data.Team}-{data.SeedPackage}-{data.SeedVersion}");
        }
    }
}
