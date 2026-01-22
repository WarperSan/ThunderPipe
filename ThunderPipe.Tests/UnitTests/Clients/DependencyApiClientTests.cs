using ThunderPipe.Clients;
using ThunderPipe.Models.API.GetDependency;
using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Clients;

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

		using var client = new DependencyApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var requested = new[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(requested);

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

		using var client = new DependencyApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var requested = new[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(requested);

		// Assert
		var expected = new[] { SLUG_2, SLUG_3 };

		Assert.Equal(missing.Count, expected.Length);

		foreach (var slug in expected)
			Assert.True(missing.Contains(slug));
	}

	[Fact]
	public async Task GetMissing_WhenEmpty_ShouldSkipRequest()
	{
		// Arrange
		const string URL = "https://google.com";

		var mockHttp = new MockHttpMessageHandler();
		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new DependencyApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var missing = await client.GetMissing([]);

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

		using var client = new DependencyApiClient(
			builder,
			mockHttp.ToHttpClient(),
			CancellationToken.None
		);

		// Act
		var requested = new[] { SLUG_1, SLUG_2, SLUG_3 };
		var missing = await client.GetMissing(requested);

		// Assert
		var expected = new[] { SLUG_1, SLUG_2 };

		Assert.Equal(missing.Count, expected.Length);

		foreach (var slug in expected)
			Assert.True(missing.Contains(slug));
	}
}
