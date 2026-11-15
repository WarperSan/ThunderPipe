using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.DTOs.GetPackage;
using ThunderPipe.Core.Models;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

public class PackageApiClientTests
{
	[Fact]
	public async Task GetVersion_WhenFound_ReturnVersion()
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
		Assert.Equal(VERSION, version);
	}

	[Fact]
	public async Task GetVersion_WhenNotFound_ReturnNull()
	{
		const string URL = "http://localhost:5050";
		const string TEAM = "Skeletogne";
		const string NAME = "EnemyAbilities";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").RespondError(HttpStatusCode.NotFound, "Not Found.");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		var client = new PackageApiClient(mockHttp.ToHttpClient(), builder);

		var version = await client.GetVersion(TEAM, NAME, TestContext.Current.CancellationToken);

		Assert.Null(version);
	}

	[Fact]
	public async Task GetVersion_WhenFoundEvenInvalid_ReturnRawVersion()
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
		PackageVersion rawVersion = VERSION;
		Assert.Equal(rawVersion, version);
	}
}
