using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests;

internal static class MockHttpMessageHandlerExtensions
{
	/// <summary>
	/// Creates a <see cref="HttpApiClient"/> instance using this <see cref="MockHttpMessageHandler"/>
	/// </summary>
	public static HttpApiClient ToApiClient(this MockHttpMessageHandler handler)
	{
		var client = handler.ToHttpClient();

		return new HttpApiClient(client);
	}
}
