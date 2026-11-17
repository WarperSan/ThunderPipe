using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.DTOs.GetPackage;
using ThunderPipe.Core.Models;
using ThunderPipe.Core.Tests.Infrastructure;
using ThunderPipe.Core.Tests.Infrastructure.Fixtures;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_CLIENT)]
public class PackageApiClientTests : IClassFixture<HttpFixture>
{
	private readonly HttpFixture _fixture;

	public PackageApiClientTests(HttpFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task GetVersion_WhenFound_ReturnVersion()
	{
		const string TEAM = "Skeletogne";
		const string NAME = "EnemyAbilities";
		const string VERSION = "1.0.0";

		var mockHttp = _fixture.MockHttp();

		mockHttp
			.Expect("*")
			.RespondJSON(
				new Response { LatestPackage = new Response.LatestModel { Version = VERSION } }
			);

		var client = new PackageApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var version = await client.GetVersion(TEAM, NAME, TestContext.Current.CancellationToken);

		Assert.NotNull(version);
		Assert.Equal(VERSION, version);
	}

	[Fact]
	public async Task GetVersion_WhenNotFound_ReturnNull()
	{
		const string TEAM = "Skeletogne";
		const string NAME = "EnemyAbilities";

		var mockHttp = _fixture.MockHttp();

		mockHttp.Expect("*").RespondError(HttpStatusCode.NotFound, "Not Found.");

		var client = new PackageApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var version = await client.GetVersion(TEAM, NAME, TestContext.Current.CancellationToken);

		Assert.Null(version);
	}

	[Fact]
	public async Task GetVersion_WhenFoundEvenInvalid_ReturnRawVersion()
	{
		const string TEAM = "Skeletogne";
		const string NAME = "EnemyAbilities";
		const string VERSION = "1.0.0-beta.10";

		var mockHttp = _fixture.MockHttp();

		mockHttp
			.Expect("*")
			.RespondJSON(
				new Response { LatestPackage = new Response.LatestModel { Version = VERSION } }
			);

		var client = new PackageApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var version = await client.GetVersion(TEAM, NAME, TestContext.Current.CancellationToken);

		Assert.NotNull(version);
		PackageVersion rawVersion = VERSION;
		Assert.Equal(rawVersion, version);
	}
}
