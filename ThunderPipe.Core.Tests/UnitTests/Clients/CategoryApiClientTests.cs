using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Models.Web.GetCategory;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

public class CategoryApiClientTests
{
	[Fact]
	public async Task GetMissing_WhenAllFound_ShouldReturnEmpty()
	{
		// Arrange
		const string URL_1 = "http://localhost:5050";
		const string URL_2 = "http://localhost:8080/?cursor=abc";
		const string URL_3 = "http://localhost:8080/?cursor=def";

		const string SLUG_1 = "mixtapes";
		const string SLUG_2 = "asset-replacements";
		const string SLUG_3 = "afflictions";

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

		using var client = new CategoryApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new Category[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		// Assert
		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenEmpty_ShouldSkipRequest()
	{
		// Arrange
		const string URL_1 = "https://google.com";

		var mockHttp = new MockHttpMessageHandler();
		var builder = new RequestBuilder().ToUri(new Uri(URL_1));

		using var client = new CategoryApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var missing = await client.GetMissing([], "test", TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenNextPageIsNull_ShouldStop()
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

		using var client = new CategoryApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new Category[] { "test" };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		// Assert
		Assert.Equal(missing.Count, requested.Length);

		foreach (var slug in requested)
			Assert.Contains(slug, missing);
	}

	[Fact]
	public async Task GetMissing_WhenNextPageWasVisited_ShouldStop()
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

		using var client = new CategoryApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new Category[] { "temp" };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		// Assert
		Assert.Equal(missing.Count, requested.Length);

		foreach (var slug in requested)
			Assert.Contains(slug, missing);
	}

	[Fact]
	public async Task GetMissing_WhenInvalidNextPage_ShouldStop()
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

		using var client = new CategoryApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new Category[] { "temp", "test2" };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		// Assert
		Assert.Equal(missing.Count, requested.Length);

		foreach (var slug in requested)
			Assert.Contains(slug, missing);
	}
}
