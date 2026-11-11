using ThunderPipe.Core.IntegrationTests.Infrastructure;
using ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;
using Xunit;

namespace ThunderPipe.Core.IntegrationTests.Tests;

[Collection(ThunderstoreCollection.NAME)]
[Trait("Category", "Integration")]
public sealed class CommunityApiClientTests(ThunderstoreFixture service)
{
    [Fact]
    public async Task GetMissing_WhenFound_ReturnEmpty()
    {
        var missing = await service.Communities.GetMissing(
            [Settings.ExistingCommunity],
            TestContext.Current.CancellationToken
        );
        Assert.Empty(missing);
    }

    [Fact]
    public async Task GetMissing_WhenNotFound_ReturnMissing()
    {
        var community = CommunityGenerator.GenerateMissing();
        var missing = await service.Communities.GetMissing(
            [community],
            TestContext.Current.CancellationToken
        );
        Assert.Single(missing, community);
    }

    [Fact]
    public async Task GetMissing_WhenFoundSome_ReturnMissing()
    {
        var community = CommunityGenerator.GenerateMissing();
        var missing = await service.Communities.GetMissing(
            [Settings.ExistingCommunity, community],
            TestContext.Current.CancellationToken
        );
        Assert.Single(missing, community);
    }

    [Fact]
    public async Task Exists_WhenFound_ReturnTrue()
    {
        var exists = await service.Communities.Exists(
            Settings.ExistingCommunity,
            TestContext.Current.CancellationToken
        );
        Assert.True(exists);
    }

    [Fact]
    public async Task Exists_WhenNotFound_ReturnFalse()
    {
        var community = CommunityGenerator.GenerateMissing();
        var exists = await service.Communities.Exists(
            community,
            TestContext.Current.CancellationToken
        );
        Assert.False(exists);
    }
}
