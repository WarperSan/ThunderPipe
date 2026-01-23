using ThunderPipe.Clients;
using ThunderPipe.Utils;

namespace ThunderPipe.Tests.MockedObjects;

/// <summary>
/// Version of <see cref="ThunderstoreClient"/> that has methods to test certain behaviors
/// </summary>
internal class TestClient : ThunderstoreClient
{
	/// <inheritdoc />
	public TestClient(RequestBuilder builder, HttpClient client, CancellationToken ct)
		: base(builder, client, ct) { }

	/// <summary>
	/// Sends a request and tries to ensure the response is a success
	/// </summary>
	public async Task TryReceiveSuccess()
	{
		var request = Builder.Build();

		var response = await SendRequest(request);

		response.EnsureSuccessStatusCode();
	}

	/// <summary>
	/// Sends a request and tries to parse the JSON response
	/// </summary>
	public Task<T> TryReceiveJson<T>()
	{
		var request = Builder.Build();

		return SendRequest<T>(request);
	}
}
