using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.DTOs.GetCommunity;
using ThunderPipe.Core.Models;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

public class CommunityApiClientTests
{
	[Fact]
	public async Task Exists_WhenFound_ReturnTrue()
	{
		const string URL_1 = "http://localhost:5050";
		const string URL_2 = "http://localhost:8080/?cursor=abc";
		const string URL_3 = "http://localhost:8080/?cursor=def";

		const string SLUG_1 = "gorebox";
		const string SLUG_2 = "rv-there-yet";
		const string SLUG_3 = "inscryption";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp
			.Expect(URL_1 + "/*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [new Response.PageItemModel { Slug = SLUG_1 }],
				}
			);

		mockHttp
			.Expect(URL_2)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_3 },
					Items = [new Response.PageItemModel { Slug = SLUG_2 }],
				}
			);

		mockHttp
			.Expect(URL_3)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = null },
					Items = [new Response.PageItemModel { Slug = SLUG_3 }],
				}
			);

		var builder = new RequestBuilder().ToUri(new Uri(URL_1));

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), builder);

		var doesCommunityExist = await client.Exists(SLUG_3, TestContext.Current.CancellationToken);

		Assert.True(doesCommunityExist);
	}

	[Fact]
	public async Task Exists_WhenNextPageIsNull_ShouldStop()
	{
		const string URL_1 = "https://google.com";
		const string URL_2 = "localhost:8080/?cursor=abc";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp
			.Expect(URL_1 + "/*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [],
				}
			);

		mockHttp
			.Expect(URL_2)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = null },
					Items = [],
				}
			);

		var builder = new RequestBuilder().ToUri(new Uri(URL_1));

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), builder);

		var wasCommunityFound = await client.Exists("test", TestContext.Current.CancellationToken);

		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task Exists_WhenNextPageWasVisited_ShouldStop()
	{
		const string URL_1 = "http://localhost:5050";
		const string URL_2 = "http://localhost:8080/?cursor=abc";
		const string URL_3 = "http://localhost:8080/?cursor=def";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp
			.Expect(URL_1 + "/*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [],
				}
			);

		mockHttp
			.Expect(URL_2)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_3 },
					Items = [],
				}
			);

		mockHttp
			.Expect(URL_3)
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = URL_2 },
					Items = [],
				}
			);

		var builder = new RequestBuilder().ToUri(new Uri(URL_1));

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), builder);

		var wasCommunityFound = await client.Exists("test", TestContext.Current.CancellationToken);

		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task Exists_WhenInvalidNextPage_ShouldStop()
	{
		const string URL_1 = "http://localhost:5050";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp
			.Expect(URL_1 + "/*")
			.RespondJSON(
				new Response
				{
					Pagination = new Response.PaginationModel { NextPage = "www.example.com/page" },
					Items = [],
				}
			);

		var builder = new RequestBuilder().ToUri(new Uri(URL_1));

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), builder);

		var wasCommunityFound = await client.Exists("test2", TestContext.Current.CancellationToken);

		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task GetMissing_WhenNetworkError_ShouldStop()
	{
		const string URL_1 = "http://localhost:5050";
		const string URL_2 = "http://localhost:8080/?cursor=abc";

		const string SLUG_1 = "gorebox";
		const string SLUG_2 = "rv-there-yet";
		const string SLUG_3 = "inscryption";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp
			.Expect(URL_1 + "/*")
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

		mockHttp.Expect(URL_2).RespondError(HttpStatusCode.BadRequest, "bad request");

		var builder = new RequestBuilder().ToUri(new Uri(URL_1));

		var client = new CommunityApiClient(mockHttp.ToHttpClient(), builder);

		var missing = await client.GetMissing(
			[SLUG_1, SLUG_2, SLUG_3],
			TestContext.Current.CancellationToken
		);

		Assert.Single(missing, (Community)SLUG_3);
	}
}
