using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.DTOs.GetPackage;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

public class PackageApiClientTests
{
	[Fact]
	public async Task GetVersion_WhenPackageFound_ReturnVersion()
	{
		const string URL = "http://localhost:5050";
		const string TEAM = "Skeletogne";
		const string NAME = "EnemyAbilities";
		const string VERSION = "1.0.0";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp
			.Expect(URL + "/*")
			.RespondJSON(
				new Response { LatestPackage = new Response.LatestModel { Version = VERSION } }
			);

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		var client = new PackageApiClient(mockHttp.ToHttpClient(), builder);

		var version = await client.GetVersion(TEAM, NAME, TestContext.Current.CancellationToken);

		Assert.NotNull(version);
		Assert.True(version.IsValid());
		Assert.Equal(VERSION, version);
	}

	[Fact]
	public async Task GetVersion_WhenPackageFoundWithInvalidVersion_ReturnInvalidVersion()
	{
		const string URL = "http://localhost:5050";
		const string TEAM = "Skeletogne";
		const string NAME = "EnemyAbilities";
		const string VERSION = "1.0.0-beta.10";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp
			.Expect(URL + "/*")
			.RespondJSON(
				new Response { LatestPackage = new Response.LatestModel { Version = VERSION } }
			);

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		var client = new PackageApiClient(mockHttp.ToHttpClient(), builder);

		var version = await client.GetVersion(TEAM, NAME, TestContext.Current.CancellationToken);

		Assert.NotNull(version);
		Assert.False(version.IsValid());
		Assert.NotEqual(VERSION, version.ToString());
	}

	[Fact]
	public async Task GetVersion_WhenPackageNotFound()
	{
		const string URL = "http://localhost:5050";
		const string TEAM = "Skeletogne";
		const string NAME = "EnemyAbilities";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").RespondError(HttpStatusCode.NotFound, "Not found.");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		var client = new PackageApiClient(mockHttp.ToHttpClient(), builder);

		var version = await client.GetVersion(TEAM, NAME, TestContext.Current.CancellationToken);

		Assert.Null(version);
	}
}
