using ThunderPipe.Core.IntegrationTests.Infrastructure;
using ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;
using Xunit;

namespace ThunderPipe.Core.IntegrationTests.Tests;

[Collection(ThunderstoreCollection.NAME)]
[Trait("Category", "Integration")]
public sealed class DependencyApiClientTests(ThunderstoreFixture service)
{
    [Fact]
    public async Task GetMissing_WhenFound_ReturnEmpty()
    {
        var missing = await service.Dependencies.GetMissing(
            [Settings.ExistingPackage],
            TestContext.Current.CancellationToken
        );
        Assert.Empty(missing);
    }

    [Fact]
    public async Task GetMissing_WhenWrongVersion_ReturnMissing()
    {
        var version = PackageVersionGenerator.GenerateGreater(Settings.ExistingPackage.Version);
        var dependency = PackageDependencyGenerator.ChangeVersion(
            Settings.ExistingPackage,
            version
        );
        var missing = await service.Dependencies.GetMissing(
            [dependency],
            TestContext.Current.CancellationToken
        );
        Assert.Single(missing, dependency);
    }

    [Fact]
    public async Task GetMissing_WhenWrongName_ReturnMissing()
    {
        var dependency = PackageDependencyGenerator.ChangeName(
            Settings.ExistingPackage,
            PackageNameGenerator.GenerateMissing()
        );
        var missing = await service.Dependencies.GetMissing(
            [dependency],
            TestContext.Current.CancellationToken
        );
        Assert.Single(missing, dependency);
    }

    [Fact]
    public async Task GetMissing_WhenWrongTeam_ReturnMissing()
    {
        var dependency = PackageDependencyGenerator.ChangeTeam(
            Settings.ExistingPackage,
            TeamGenerator.GenerateMissing()
        );
        var missing = await service.Dependencies.GetMissing(
            [dependency],
            TestContext.Current.CancellationToken
        );
        Assert.Single(missing, dependency);
    }
}
