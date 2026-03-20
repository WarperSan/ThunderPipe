using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Models.Web;

namespace ThunderPipe.Core.Tests.MockedObjects;

/// <summary>
/// Version of <see cref="ThunderstoreClient"/> that has methods to test certain behaviors
/// </summary>
internal class TestClient : ThunderstoreClient
{
	/// <summary>
	/// Sends a request and tries to ensure the response is a success
	/// </summary>
	public async Task TryReceiveSuccess()
	{
		var request = Builder.Build();

		var response = await SendRequest(request, CancellationToken.None);

		response.EnsureSuccessStatusCode();
	}

	/// <summary>
	/// Sends a request and tries to parse the JSON response
	/// </summary>
	public Task<Response<T>> TryReceiveJson<T>()
		where T : class
	{
		var request = Builder.Build();

		return SendRequest<T>(request, CancellationToken.None);
	}
}
