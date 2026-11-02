using ThunderPipe.Core.IntegrationTests.Infrastructure;
using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Tests;

[Collection(ThunderstoreCollection.NAME)]
[Trait("Category", "Integration")]
public sealed class CommunityApiClientTests(ThunderstoreFixture service)
{
	private static Community Unknown() => $"no-such-community-{Guid.NewGuid():N}";

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
		var community = Unknown();

		var missing = await service.Communities.GetMissing(
			[community],
			TestContext.Current.CancellationToken
		);

		Assert.Single(missing);
		Assert.Contains(community, missing);
	}

	[Fact]
	public async Task GetMissing_WhenFoundSome_ReturnMissing()
	{
		var community = Unknown();

		var missing = await service.Communities.GetMissing(
			[Settings.ExistingCommunity, community],
			TestContext.Current.CancellationToken
		);

		Assert.Single(missing);
		Assert.Contains(community, missing);
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
		var community = Unknown();

		var exists = await service.Communities.Exists(
			community,
			TestContext.Current.CancellationToken
		);

		Assert.False(exists);
	}
}
