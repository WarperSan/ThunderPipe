using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.DTOs.GetCategory;
using ThunderPipe.Core.Models;
using ThunderPipe.Core.Tests.Infrastructure;
using ThunderPipe.Core.Tests.Infrastructure.Fixtures;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_CLIENT)]
public class CategoryApiClientTests : IClassFixture<HttpFixture>
{
	private readonly HttpFixture _fixture;

	public CategoryApiClientTests(HttpFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task GetMissing_WhenAllFound_ShouldReturnEmpty()
	{
		const string URL_2 = "http://localhost:8080/?cursor=abc";
		const string URL_3 = "http://localhost:8080/?cursor=def";

		const string SLUG_1 = "mixtapes";
		const string SLUG_2 = "asset-replacements";
		const string SLUG_3 = "afflictions";

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

		var client = new CategoryApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new Category[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenEmpty_ShouldSkipRequest()
	{
		var mockHttp = _fixture.MockHttp();

		var client = new CategoryApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var missing = await client.GetMissing([], "test", TestContext.Current.CancellationToken);

		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenNextPageIsNull_ShouldStop()
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

		var client = new CategoryApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new Category[] { "test" };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		Assert.Missing(requested, missing);
	}

	[Fact]
	public async Task GetMissing_WhenNextPageWasVisited_ShouldStop()
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

		var client = new CategoryApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new Category[] { "temp" };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		Assert.Missing(requested, missing);
	}

	[Fact]
	public async Task GetMissing_WhenInvalidNextPage_ShouldStop()
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

		var client = new CategoryApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new Category[] { "temp", "test2" };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		Assert.Missing(requested, missing);
	}

	[Fact]
	public async Task GetMissing_WhenNetworkError_ShouldStop()
	{
		const string URL_2 = "http://localhost:8080/?cursor=abc";

		const string SLUG_1 = "mixtapes";
		const string SLUG_2 = "asset-replacements";
		const string SLUG_3 = "afflictions";

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

		var client = new CategoryApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new Category[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(
			requested,
			"test",
			TestContext.Current.CancellationToken
		);

		Assert.Single(missing, (Category)SLUG_3);
	}
}
