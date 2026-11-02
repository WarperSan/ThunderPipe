namespace ThunderPipe.Core.IntegrationTests.Infrastructure;

/// <summary>
/// Class used mark a collection of fixtures used by tests
/// </summary>
[CollectionDefinition(NAME)]
public sealed class ThunderstoreCollection : ICollectionFixture<ThunderstoreFixture>
{
	/// <summary>
	/// Name of this collection
	/// </summary>
	public const string NAME = "Thunderstore";
}
