using System.Net;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Models.Web.GetDependency;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.UnitTests.Clients;

// TODO: Get 100% coverage on DependencyApiClient
public class DependencyApiClientTests
{
	[Fact]
	public async Task GetMissing_WhenAllFound_ShouldReturnEmpty()
	{
		// Arrange
		const string URL = "http://localhost:5050";

		const string SLUG_1 = "ItsEmul-PEAKValuables-1.2.0";
		const string SLUG_2 = "AtomicStudio-BetterScoutReport-0.1.6";
		const string SLUG_3 = "SavageCore-PEAK_BritishEnglish_Translation-0.2.1";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").RespondJSON(new Response { IsActive = true });

		mockHttp.Expect(URL + "/*").RespondJSON(new Response { IsActive = true });

		mockHttp.Expect(URL + "/*").RespondJSON(new Response { IsActive = true });

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new DependencyApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new[]
		{
			new PackageDependency(SLUG_1),
			new PackageDependency(SLUG_2),
			new PackageDependency(SLUG_3),
		};
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenSomeFound_ShouldReturnRemaining()
	{
		// Arrange
		const string URL = "http://localhost:5050";

		const string SLUG_1 = "ItsEmul-PEAKValuables-1.2.0";
		const string SLUG_2 = "AtomicStudio-BetterScoutReport-0.1.6";
		const string SLUG_3 = "SavageCore-PEAK_BritishEnglish_Translation-0.2.1";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").RespondJSON(new Response { IsActive = true });

		mockHttp.Expect(URL + "/*").RespondJSON(new Response { IsActive = false });

		mockHttp.Expect(URL + "/*").RespondJSON(new Response { IsActive = false });

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new DependencyApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new[]
		{
			new PackageDependency(SLUG_1),
			new PackageDependency(SLUG_2),
			new PackageDependency(SLUG_3),
		};
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(2, missing.Count);
	}

	[Fact]
	public async Task GetMissing_WhenEmpty_ShouldSkipRequest()
	{
		// Arrange
		const string URL = "https://google.com";

		var mockHttp = new MockHttpMessageHandler();
		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new DependencyApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var missing = await client.GetMissing([], TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(missing);
	}

	[Fact]
	public async Task GetMissing_WhenInvalidFormat_ShouldSkipInvalids()
	{
		// Arrange
		const string URL = "http://localhost:5050";

		const string SLUG_1 = "ItsEmul-1.2.0";
		const string SLUG_2 = "BetterScoutReport-0.1.6";
		const string SLUG_3 = "SavageCore-PEAK_BritishEnglish_Translation-0.2.1";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").RespondJSON(new Response { IsActive = true });

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new DependencyApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new[]
		{
			new PackageDependency(SLUG_1),
			new PackageDependency(SLUG_2),
			new PackageDependency(SLUG_3),
		};
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		// Assert
		var expected = new[] { new PackageDependency(SLUG_1), new PackageDependency(SLUG_2) };

		Assert.Equal(missing.Count, expected.Length);

		foreach (var slug in expected)
			Assert.Contains(slug, missing);
	}

	[Fact]
	public async Task GetMissing_WhenErrorReturned_ShouldReturnRemaining()
	{
		// Arrange
		const string URL = "http://localhost:5050";

		const string SLUG_1 = "ItsEmul-PEAKValuables-1.2.0";
		const string SLUG_2 = "AtomicStudio-BetterScoutReport-0.1.6";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").RespondError(HttpStatusCode.NotFound, "Not found.");

		mockHttp.Expect(URL + "/*").RespondError(HttpStatusCode.NotFound, "Not found.");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new DependencyApiClient();
		client.Builder = builder;
		client.Client = mockHttp.ToHttpClient();

		// Act
		var requested = new[] { new PackageDependency(SLUG_1), new PackageDependency(SLUG_2) };
		var missing = await client.GetMissing(requested, TestContext.Current.CancellationToken);

		// Assert
		var expected = new[] { new PackageDependency(SLUG_1), new PackageDependency(SLUG_2) };

		Assert.Equal(missing.Count, expected.Length);

		foreach (var slug in expected)
			Assert.Contains(slug, missing);
	}
}
