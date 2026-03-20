using System.Net;
using System.Net.Mime;
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
		return source.Respond(MediaTypeNames.Application.Json, json);
	}

	/// <summary>
	/// Sets the response to the given errors
	/// </summary>
	public static MockedRequest RespondErrors(
		this MockedRequest source,
		IDictionary<string, string[]> errors
	)
	{
		var json = JsonConvert.SerializeObject(errors);
		return source.Respond(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, json);
	}

	/// <summary>
	/// Sets the response to the given error
	/// </summary>
	public static MockedRequest RespondError(
		this MockedRequest source,
		HttpStatusCode status,
		string error
	)
	{
		var json = JsonConvert.SerializeObject(new { details = error });
		return source.Respond(status, MediaTypeNames.Application.Json, json);
	}
}
