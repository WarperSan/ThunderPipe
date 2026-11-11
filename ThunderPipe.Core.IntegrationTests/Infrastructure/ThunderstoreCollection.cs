using Xunit;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure;

/// <summary>Collection shared by all live Thunderstore integration tests.</summary>
[CollectionDefinition(NAME)]
public sealed class ThunderstoreCollection : ICollectionFixture<ThunderstoreFixture>
{
    public const string NAME = "Thunderstore";
}
