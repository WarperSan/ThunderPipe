using Newtonsoft.Json;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.UnitTests.Utils;

public class HttpApiClientTests
{
	private class Person
	{
		[JsonProperty("first_name")]
		[JsonRequired]
		public string FirstName { get; private set; } = "";

		[JsonProperty("last_name")]
		[JsonRequired]
		public string LastName { get; private set; } = "";
	}

	[Fact]
	public async Task SendRequest_WhenInvalidJson_ThrowException()
	{
		const string URL = "http://localhost:5050";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").Respond("application/json", "[}");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		var client = new HttpApiClient(mockHttp.ToHttpClient());

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			var request = builder.Build();

			await client.SendRequest<Person>(request, CancellationToken.None);
		});

		Assert.NotNull(ex.InnerException);
		Assert.IsType<JsonSerializationException>(ex.InnerException);
	}

	[Fact]
	public async Task SendRequest_WhenRequiredFieldMising_ThrowException()
	{
		const string URL = "http://localhost:5050";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").Respond("application/json", "{}");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		var client = new HttpApiClient(mockHttp.ToHttpClient());

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			var request = builder.Build();

			await client.SendRequest<Person>(request, CancellationToken.None);
		});

		Assert.NotNull(ex.InnerException);
		Assert.IsType<JsonSerializationException>(ex.InnerException);
	}

	[Fact]
	public async Task SendRequest_WhenParsedNull_ThrowException()
	{
		const string URL = "http://localhost:5050";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").Respond("application/json", "");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		var client = new HttpApiClient(mockHttp.ToHttpClient());

		await Assert.ThrowsAsync<NullReferenceException>(async () =>
		{
			var request = builder.Build();

			await client.SendRequest<Person>(request, CancellationToken.None);
		});
	}

	[Fact]
	public async Task Dispose_WhenDisposed_ThrowException()
	{
		var client = new HttpClient();
		var apiClient = new HttpApiClient(client);

		client.Dispose();

		await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
		{
			var request = new RequestBuilder().Build();

			await apiClient.SendRequest(request, CancellationToken.None);
		});
	}
}
