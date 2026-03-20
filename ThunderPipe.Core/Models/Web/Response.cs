using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThunderPipe.Core.Models.Web;

/// <summary>
/// Wrapper to handle <see cref="HttpResponseMessage"/> better
/// </summary>
internal sealed class Response<T>
	where T : class
{
	private Response() { }

	/// <summary>
	/// JSON content of the response if success
	/// </summary>
	public T? Data { get; private set; }

	/// <summary>
	/// Creates a new instance of <see cref="Response{T}"/> from the given response
	/// </summary>
	public static async Task<Response<T>> CreateResponse(
		HttpResponseMessage response,
		CancellationToken ct
	)
	{
		var status = response.StatusCode;
		var content = await response.Content.ReadAsStringAsync(ct);

		if (status == HttpStatusCode.OK)
		{
			// Parse normally
			var data = ParseJson<T>(content);
		}

		if (status == HttpStatusCode.BadRequest)
		{
			var jObject = JObject.Parse(content);

			if (jObject.Type == JTokenType.Object)
			{
				// if object, parse every error as field specific
			}

			if (jObject.Type == JTokenType.Array)
			{
				// if array, parse all errors as global
			}

			throw new InvalidOperationException(
				$"Cannot parse a '{status:D}' response when it's not an array or an object."
			);
		}

		// Parse details
	}

	private static TPayload ParseJson<TPayload>(string content)
	{
		TPayload? json;

		try
		{
			json = JsonConvert.DeserializeObject<TPayload>(content);
		}
		catch (JsonException e)
		{
			throw new InvalidOperationException(
				$"Failed to deserialize the response: \n\n{content}",
				e
			);
		}

		if (json == null)
			throw new NullReferenceException(
				$"Failed to parse the response's payload as '{nameof(T)}'"
			);

		return json;
	}
}
