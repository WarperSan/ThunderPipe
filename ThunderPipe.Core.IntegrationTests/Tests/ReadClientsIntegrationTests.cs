using ThunderPipe.Core.Models;
using Xunit;
using ThunderPipe.Core.IntegrationTests.Infrastructure;

namespace ThunderPipe.Core.IntegrationTests;

/// <summary>Real HTTP requests against deterministic seeded Django data.</summary>
[Collection(ThunderstoreCollection.NAME)]
[Trait("Category", "Integration")]
public sealed class ReadClientsIntegrationTests
{
	private readonly ThunderstoreFixture _env;

	public ReadClientsIntegrationTests(ThunderstoreFixture env) => _env = env;

	[Fact]
	public async Task Community_GetMissing_ExistingAndAbsent_AcrossPages()
	{
		// The upstream create_test_data command generates 105 communities. Request ALL of them,
		// so a client that stops after the first 100-item page cannot pass,
		// regardless of the server's ordering of records.
		var existing = _env.Data.PageCommunities
			.Select(identifier => (Community)identifier)
			.ToArray();
		Assert.Equal(105, existing.Length);
		Community missing = "tp-integration-missing-community";
		var requested = existing.Append(missing).ToArray();

		var absent = await _env.Communities.GetMissing(
			requested,
			TestContext.Current.CancellationToken
		);

		Assert.Single(absent);
		Assert.Contains(missing, absent);
		Assert.All(existing, community => Assert.DoesNotContain(community, absent));
	}

	[Fact]
	public async Task Category_GetMissing_ExistingAndAbsent_AcrossPages()
	{
		// As above, require all 105 seeded categories to be discovered.
		// This makes a first-page-only implementation fail deterministically.
		var existing = Enumerable.Range(0, 105)
			.Select(i => (Category)$"tp-integration-page-{i:000}")
			.ToArray();
		Category missingCategory = "tp-integration-no-category";
		var requested = existing.Append(missingCategory).ToArray();

		var missing = await _env.Categories.GetMissing(
			requested,
			_env.Data.Community,
			TestContext.Current.CancellationToken
		);

		Assert.Single(missing);
		Assert.Contains(missingCategory, missing);
		Assert.All(existing, category => Assert.DoesNotContain(category, missing));
	}

	[Fact]
	public async Task Category_GetMissing_AllFound_ReturnsEmpty()
	{
		var missing = await _env.Categories.GetMissing(
			[(Category)_env.Data.Category],
			_env.Data.Community,
			TestContext.Current.CancellationToken
		);
		Assert.Empty(missing);
	}

	[Fact]
	public async Task Package_GetVersion_AbsentPackage_FailsExplicitly()
	{
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			_env.Packages.GetVersion(
				_env.Data.Team,
				"NoSuchPackage",
				TestContext.Current.CancellationToken
			)
		);
	}

	[Fact]
	public async Task Dependency_GetMissing_DistinguishesActiveInactiveMissingAndMalformed()
	{
		var active = new PackageDependency(
			$"{_env.Data.Team}-{_env.Data.SeedPackage}-{_env.Data.SeedVersion}"
		);
		var inactive = new PackageDependency(
			$"{_env.Data.Team}-{_env.Data.SeedPackage}-{_env.Data.OldVersion}"
		);
		var nonexistent = new PackageDependency($"{_env.Data.Team}-NoSuchPackage-1.0.0");
		var malformed = new PackageDependency("invalid");
		var missing = await _env.Dependencies.GetMissing(
			[active, inactive, nonexistent, malformed],
			TestContext.Current.CancellationToken
		);
		Assert.Equal(3, missing.Count);
		Assert.DoesNotContain(active, missing);
		Assert.Contains(inactive, missing);
		Assert.Contains(nonexistent, missing);
		Assert.Contains(malformed, missing);
	}

	[Fact]
	public async Task Dependency_GetMissing_EmptyInput_ReturnsEmpty()
	{
		var missing = await _env.Dependencies.GetMissing(
			Array.Empty<PackageDependency>(),
			TestContext.Current.CancellationToken
		);
		Assert.Empty(missing);
	}

	[Fact]
	public async Task Package_GetVersion_ReturnsLatestActiveSeedVersion()
	{
		PackageVersion result = await _env.Packages.GetVersion(
			_env.Data.Team,
			_env.Data.SeedPackage,
			TestContext.Current.CancellationToken
		);
		Assert.Equal(_env.Data.SeedVersion, (string)result);
	}
}
