using Newtonsoft.Json;
using ThunderPipe.Models.API.GetCategory;
using ThunderPipe.Tests.MockedObjects;
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
			await client.TryReceiveSuccess();
		}
		catch (Exception e)
		{
			Assert.IsType<ObjectDisposedException>(e);
			return;
		}

		Assert.Fail("Should have thrown an exception");
	}
}
