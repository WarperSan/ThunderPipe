using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.MockedObjects;

/// <summary>
/// Version of <see cref="MockHttpMessageHandler"/> used for tests
/// </summary>
public sealed class TestMockHttpMessageHandler : MockHttpMessageHandler
{
	private readonly RequestBuilder _builder;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestMockHttpMessageHandler"/> class.
	/// </summary>
	public TestMockHttpMessageHandler(RequestBuilder builder)
	{
		_builder = builder;
	}

	/// <summary>
	/// Adds a request expectation at the given endpoint
	/// </summary>
	public MockedRequest Expect(string endpoint)
	{
		var builder = new RequestBuilder(_builder);
		builder.ToEndpoint(endpoint);

		var request = builder.Build();
		var uri = request.RequestUri?.AbsoluteUri ?? endpoint;

		return MockHttpMessageHandlerExtensions.Expect(this, uri);
	}
}
