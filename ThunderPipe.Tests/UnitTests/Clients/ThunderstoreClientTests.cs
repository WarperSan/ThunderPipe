using Newtonsoft.Json;
using ThunderPipe.Clients;
using ThunderPipe.Models.API.GetCategory;
using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Clients;

public class ThunderstoreClientTests
{
	// ReSharper disable once ClassNeverInstantiated.Local
	private class Person
	{
		// ReSharper disable NotAccessedPositionalProperty.Local
		public string FirstName { get; private set; } = "";

		public string LastName { get; private set; } = "";
		// ReSharper restore NotAccessedPositionalProperty.Local
	}

	private class TestClient : ThunderstoreClient
	{
		/// <inheritdoc />
		public TestClient(RequestBuilder builder, HttpClient client, CancellationToken ct)
			: base(builder, client, ct) { }

		public async Task TrySend()
		{
			var request = Builder.Build();

			var response = await SendRequest(request);

			response.EnsureSuccessStatusCode();
		}

		public Task<T> TryReceiveJson<T>()
		{
			var request = Builder.Build();

			return SendRequest<T>(request);
		}
	}

	[Fact]
	public async Task SendRequest_WhenInvalidJson_ThrowException()
	{
		const string URL = "http://localhost:5050";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").Respond("application/json", "[}");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new TestClient(builder, mockHttp.ToHttpClient(), CancellationToken.None);

		try
		{
			await client.TryReceiveJson<Response>();
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			Assert.NotNull(e.InnerException);
			Assert.IsType<JsonSerializationException>(e.InnerException);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task SendRequest_WhenRequiredFieldMising_ThrowException()
	{
		const string URL = "http://localhost:5050";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").Respond("application/json", "{}");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new TestClient(builder, mockHttp.ToHttpClient(), CancellationToken.None);

		try
		{
			await client.TryReceiveJson<Response>();
		}
		catch (Exception e)
		{
			Assert.IsType<InvalidOperationException>(e);
			Assert.NotNull(e.InnerException);
			Assert.IsType<JsonSerializationException>(e.InnerException);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task SendRequest_WhenParsedNull_ThrowException()
	{
		const string URL = "http://localhost:5050";

		var mockHttp = new MockHttpMessageHandler();

		mockHttp.Expect(URL + "/*").Respond("application/json", "");

		var builder = new RequestBuilder().ToUri(new Uri(URL));

		using var client = new TestClient(builder, mockHttp.ToHttpClient(), CancellationToken.None);

		try
		{
			await client.TryReceiveJson<Person>();
		}
		catch (Exception e)
		{
			Assert.IsType<NullReferenceException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}

	[Fact]
	public async Task Dispose_WhenDisposed_ThrowException()
	{
		var client = new TestClient(new RequestBuilder(), new HttpClient(), CancellationToken.None);

		client.Dispose();

		try
		{
			await client.TrySend();
		}
		catch (Exception e)
		{
			Assert.IsType<ObjectDisposedException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}
}
