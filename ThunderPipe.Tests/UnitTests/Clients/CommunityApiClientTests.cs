using ThunderPipe.Clients;
using ThunderPipe.Models.API.GetCommunity;
using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Clients;

public class CommunityApiClientTests
{
	[Fact]
	public async Task Exists_WhenFound_ReturnTrue()
	{
		// Arrange
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

		using var client = new CommunityApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var doesCommunityExist = await client.Exists(SLUG_3);

		// Assert
		Assert.True(doesCommunityExist);
	}

	[Fact]
	public async Task Exists_WhenNextPageIsNull_ShouldStop()
	{
		// Arrange
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

		using var client = new CommunityApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var wasCommunityFound = await client.Exists("test");

		// Assert
		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task Exists_WhenNextPageWasVisited_ShouldStop()
	{
		// Arrange
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

		using var client = new CommunityApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var wasCommunityFound = await client.Exists("test");

		// Assert
		Assert.False(wasCommunityFound);
	}

	[Fact]
	public async Task Exists_WhenInvalidNextPage_ShouldStop()
	{
		// Arrange
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

		using var client = new CommunityApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var wasCommunityFound = await client.Exists("test2");

		// Assert
		Assert.False(wasCommunityFound);
	}
}
