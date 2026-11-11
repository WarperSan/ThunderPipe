using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure;

/// <summary>
/// Shared integration settings, following the maintainer's environment-variable
/// convention. Extra settings are required only by publish/validation/paging tests.
/// </summary>
public static class Settings
{
    private static string GetRequired(string name) =>
        Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException($"No value was assigned to the required variable '{name}'.");

    public static readonly Uri BaseUri = new(GetRequired("THUNDERSTORE_URL"));
    public static readonly Community ExistingCommunity = new(GetRequired("THUNDERSTORE_COMMUNITY"));
    public static readonly PackageDependency ExistingPackage = new(GetRequired("THUNDERSTORE_PACKAGE"));

    public static FixtureData Supplemental => new(
        GetRequired("THUNDERSTORE_TOKEN"),
        GetRequired("THUNDERSTORE_TEAM"),
        GetRequired("THUNDERSTORE_COMMUNITY"),
        GetRequired("THUNDERSTORE_CATEGORY"),
        GetRequired("THUNDERSTORE_SEED_PACKAGE"),
        GetRequired("THUNDERSTORE_SEED_VERSION"),
        GetRequired("THUNDERSTORE_OLD_VERSION"),
        GetRequired("THUNDERSTORE_PAGE_COMMUNITIES")
            .Split(',', StringSplitOptions.RemoveEmptyEntries));
}

public sealed record FixtureData(
    string Token,
    string Team,
    string Community,
    string Category,
    string SeedPackage,
    string SeedVersion,
    string OldVersion,
    string[] PageCommunities);
