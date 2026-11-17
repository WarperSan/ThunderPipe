using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.DTOs.GetCommunity;
using ThunderPipe.Core.Models;
using ThunderPipe.Core.Tests.Infrastructure;
using ThunderPipe.Core.Tests.Infrastructure.Fixtures;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_CLIENT)]
public class CommunityApiClientTests : IClassFixture<HttpFixture>
{
	private readonly HttpFixture _fixture;

	public CommunityApiClientTests(HttpFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task Exists_WhenFound_ReturnTrue()
	{
		const string URL_2 = "http://localhost:8080/?cursor=abc";
		const string URL_3 = "http://localhost:8080/?cursor=def";

		const string SLUG_1 = "gorebox";
		const string SLUG_2 = "rv-there-yet";
		const string SLUG_3 = "inscryption";

		var mockHttp = _fixture.MockHttp();

		mockHttp
			.Expect("*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [new Response.PageItemModel { Slug = SLUG_1 }],
				}
			);

		mockHttp
			.When(URL_2)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_3 },
					Items = [new Response.PageItemModel { Slug = SLUG_2 }],
				}
			);

		mockHttp
			.When(URL_3)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = null },
					Items = [new Response.PageItemModel { Slug = SLUG_3 }],
				}
			);

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var doesCommunityExist = await client.Exists(SLUG_3, TestContext.Current.CancellationToken);

		Assert.True(doesCommunityExist);
	}

	[Fact]
	public async Task Exists_WhenNextPageIsNull_ShouldStop()
	{
		const string URL_2 = "localhost:8080/?cursor=abc";

		var mockHttp = _fixture.MockHttp();

		mockHttp
			.Expect("*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [],
				}
			);

		mockHttp
			.When(URL_2)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = null },
					Items = [],
				}
			);

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var wasCommunityFound = await client.Exists("test", TestContext.Current.CancellationToken);

		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task Exists_WhenNextPageWasVisited_ShouldStop()
	{
		const string URL_2 = "http://localhost:8080/?cursor=abc";
		const string URL_3 = "http://localhost:8080/?cursor=def";

		var mockHttp = _fixture.MockHttp();

		mockHttp
			.Expect("*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [],
				}
			);

		mockHttp
			.When(URL_2)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_3 },
					Items = [],
				}
			);

		mockHttp
			.When(URL_3)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [],
				}
			);

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var wasCommunityFound = await client.Exists("test", TestContext.Current.CancellationToken);

		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task Exists_WhenInvalidNextPage_ShouldStop()
	{
		var mockHttp = _fixture.MockHttp();

		mockHttp
			.Expect("*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = "www.example.com/page" },
					Items = [],
				}
			);

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var wasCommunityFound = await client.Exists("test2", TestContext.Current.CancellationToken);

		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task GetMissing_WhenNetworkError_ShouldStop()
	{
		const string URL_2 = "http://localhost:8080/?cursor=abc";

		const string SLUG_1 = "gorebox";
		const string SLUG_2 = "rv-there-yet";
		const string SLUG_3 = "inscryption";

		var mockHttp = _fixture.MockHttp();

		mockHttp
			.Expect("*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items =
					[
						new Response.PageItemModel { Slug = SLUG_1 },
						new Response.PageItemModel { Slug = SLUG_2 },
					],
				}
			);

		mockHttp.When(URL_2).RespondError(HttpStatusCode.BadRequest, "bad request");

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var missing = await client.GetMissing(
			[SLUG_1, SLUG_2, SLUG_3],
			TestContext.Current.CancellationToken
		);

		Assert.Single(missing, (Community)SLUG_3);
	}
}
