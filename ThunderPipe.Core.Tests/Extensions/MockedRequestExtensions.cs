using System.Net;
using Newtonsoft.Json;

namespace ThunderPipe.Core.Tests;

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

	/// <summary>
	/// Sets the response of the current <see cref="MockedRequest"/>, with the given status.
	/// </summary>
	public static MockedRequest RespondStatus(this MockedRequest source, HttpStatusCode status)
	{
		return source.Respond(status);
	}
}
