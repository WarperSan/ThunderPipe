using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.DTOs.GetDependency;
using ThunderPipe.Core.Models;
using ThunderPipe.Core.Tests.Infrastructure;
using ThunderPipe.Core.Tests.Infrastructure.Fixtures;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

// TODO: Get 100% coverage on DependencyApiClient
[Trait(TestTraits.CATEGORY_NAME, TestTraits.CATEGORY_CLIENT)]
public class DependencyApiClientTests : IClassFixture<HttpFixture>
{
	private readonly HttpFixture _fixture;

	public DependencyApiClientTests(HttpFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task GetMissing_WhenAllFound_ShouldReturnEmpty()
	{
		const string SLUG_1 = "ItsEmul-PEAKValuables-1.2.0";
		const string SLUG_2 = "AtomicStudio-BetterScoutReport-0.1.6";
		const string SLUG_3 = "SavageCore-PEAK_BritishEnglish_Translation-0.2.1";

		var mockHttp = _fixture.MockHttp();

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		var client = new DependencyApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new PackageDependency[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenSomeFound_ShouldReturnRemaining()
	{
		const string SLUG_1 = "ItsEmul-PEAKValuables-1.2.0";
		const string SLUG_2 = "AtomicStudio-BetterScoutReport-0.1.6";
		const string SLUG_3 = "SavageCore-PEAK_BritishEnglish_Translation-0.2.1";

		var mockHttp = _fixture.MockHttp();

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = false });

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = false });

		var client = new DependencyApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new PackageDependency[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		Assert.Equal(2, missing.Count);
	}

	[Fact]
	public async Task GetMissing_WhenEmpty_ShouldSkipRequest()
	{
		var mockHttp = _fixture.MockHttp();

		var client = new DependencyApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var missing = await client.GetMissing([], TestContext.Current.CancellationToken);

		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenInvalidFormat_ShouldSkipInvalids()
	{
		const string SLUG_1 = "ItsEmul-1.2.0";
		const string SLUG_2 = "BetterScoutReport-0.1.6";
		const string SLUG_3 = "SavageCore-PEAK_BritishEnglish_Translation-0.2.1";

		var mockHttp = _fixture.MockHttp();

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		var client = new DependencyApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new PackageDependency[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		var expected = new PackageDependency[] { SLUG_1, SLUG_2 };

		Assert.Missing(expected, missing);
	}

	[Fact]
	public async Task GetMissing_WhenErrorReturned_ShouldReturnRemaining()
	{
		const string SLUG_1 = "ItsEmul-PEAKValuables-1.2.0";
		const string SLUG_2 = "AtomicStudio-BetterScoutReport-0.1.6";

		var mockHttp = _fixture.MockHttp();

		mockHttp.Expect("*").RespondError(HttpStatusCode.NotFound, "Not found.");

		mockHttp.Expect("*").RespondError(HttpStatusCode.NotFound, "Not found.");

		var client = new DependencyApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new PackageDependency[] { SLUG_1, SLUG_2 };
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		var expected = new PackageDependency[] { SLUG_1, SLUG_2 };

		Assert.Missing(expected, missing);
	}

	[Fact]
	public async Task GetMissing_WhenMultipleInvalidFormat_ShouldReturnAllInvalid()
	{
		const string SLUG_1 = "ItsEmul-1.2.0";
		const string SLUG_2 = "BetterScoutReport-0.1.6";
		const string SLUG_3 = "SavageCore-PEAK_BritishEnglish_Translation-0.2.1";

		var mockHttp = _fixture.MockHttp();

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		var client = new DependencyApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new PackageDependency[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		var expected = new PackageDependency[] { SLUG_1, SLUG_2 };

		Assert.Missing(expected, missing);
	}

	[Fact]
	public async Task GetMissing_WhenValidDuplicates_ShouldRequestOnce()
	{
		const string SLUG_1 = "bcs4313-Lethal_Gorilla-1.0.3";
		const string SLUG_2 = "TheCaptainToaster-618_Eventide-1.1.12";

		var mockHttp = _fixture.MockHttp();

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		mockHttp.Expect("*").RespondJSON(new Response { IsActive = true });

		var client = new DependencyApiClient(mockHttp.ToHttpClient(), _fixture.Builder());

		var requested = new PackageDependency[] { SLUG_1, SLUG_1, SLUG_2 };
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		Assert.Empty(missing);
	}
}
