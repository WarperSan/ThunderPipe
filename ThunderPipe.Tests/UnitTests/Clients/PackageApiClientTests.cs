using ThunderPipe.Clients;
using ThunderPipe.Models.API.GetPackage;
using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Clients;

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

		using var client = new PackageApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		var version = await client.GetVersion(TEAM, NAME);

		Assert.Equal(VERSION, version);
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

		using var client = new PackageApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		var version = await client.GetVersion(TEAM, NAME);

		Assert.Equal(VERSION, version);
	}
}
