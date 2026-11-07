using ThunderPipe.Core.Models;
using Xunit;

namespace ThunderPipe.Core.IntegrationTests;

/// <summary>Real HTTP requests against deterministic seeded Django data.</summary>
public sealed class ReadClientsIntegrationTests : IClassFixture<IntegrationEnvironment>
{
	private readonly IntegrationEnvironment _env;

	public ReadClientsIntegrationTests(IntegrationEnvironment env) => _env = env;

	[Fact]
	public async Task Community_GetMissing_ExistingAndAbsent_AcrossPages()
	{
		Community present = _env.Data.Community;
		Community missing = "tp-integration-missing-community";
		var absent = await _env.Communities.GetMissing(
			[present, missing],
			TestContext.Current.CancellationToken
		);
		Assert.Single(absent);
		Assert.Contains(missing, absent);
	}

	[Fact]
	public async Task Community_Exists_ReturnsBothOutcomes()
	{
		var ct = TestContext.Current.CancellationToken;
		Assert.True(await _env.Communities.Exists(_env.Data.Community, ct));
		Assert.False(await _env.Communities.Exists("tp-integration-not-a-community", ct));
	}

	[Fact]
	public async Task Category_GetMissing_ExistingAndAbsent_AcrossPages()
	{
		Category existing = _env.Data.Category;
		Category absentCategory = "tp-integration-no-category";
		var missing = await _env.Categories.GetMissing(
			[existing, absentCategory],
			_env.Data.Community,
			TestContext.Current.CancellationToken
		);
		Assert.Single(missing);
		Assert.Contains(absentCategory, missing);
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
