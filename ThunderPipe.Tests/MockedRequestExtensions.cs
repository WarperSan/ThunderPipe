using Newtonsoft.Json;

namespace ThunderPipe.Tests;

internal static class MockedRequestExtensions
{
	/// <summary>
	/// Sets the response of the current <see cref="T:MockedRequest"/>, with an OK (200) status code and with the JSON payload
	/// </summary>
	public static MockedRequest RespondJSON(this MockedRequest source, object content)
	{
		var json = JsonConvert.SerializeObject(content);
		return source.Respond("application/json", json);
	}
}
