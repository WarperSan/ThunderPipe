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
	public const string GLOBAL_ERRORS = "global";

	private Response(T data)
	{
		IsSuccess = true;
		Data = data;
		Errors = new Dictionary<string, IEnumerable<string>>();
	}

	private Response(IEnumerable<string> errors)
	{
		IsSuccess = false;
		Data = null;
		Errors = new Dictionary<string, IEnumerable<string>>() { [GLOBAL_ERRORS] = errors };
	}

	private Response(IDictionary<string, IEnumerable<string>> errors)
	{
		IsSuccess = false;
		Data = null;
		Errors = errors;
	}

	/// <summary>
	/// Determines if the response has been a success
	/// </summary>
	public readonly bool IsSuccess;

	/// <summary>
	/// JSON content of the response if success
	/// </summary>
	public readonly T? Data;

	/// <summary>
	/// Errors returned, grouped by category
	/// </summary>
	public readonly IDictionary<string, IEnumerable<string>> Errors;

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
			return HandleOk(content);

		var jToken = JToken.Parse(content);

		if (status == HttpStatusCode.BadRequest)
			return HandleBadRequest(jToken);

		return HandleError(jToken, content);
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

	private static Response<T> HandleOk(string content)
	{
		var data = ParseJson<T>(content);

		return new Response<T>(data);
	}

	private static Response<T> HandleBadRequest(JToken jToken)
	{
		switch (jToken)
		{
			case JObject jObject:
			{
				// if object, parse every error as field specific
				var allErrors = new Dictionary<string, IEnumerable<string>>();

				foreach (var property in jObject.Properties())
				{
					var category = property.Name;
					var errors = property.Value.Values<string>().OfType<string>();

					allErrors[category] = errors;
				}

				return new Response<T>(allErrors);
			}
			case JArray jArray:
			{
				// if array, parse all errors as global
				var errors = jArray.Values<string>().OfType<string>();

				return new Response<T>(errors);
			}
			default:
				throw new InvalidOperationException(
					$"Cannot parse a '{HttpStatusCode.BadRequest:D}' response when it's not an array or an object."
				);
		}
	}

	private static Response<T> HandleError(JToken jToken, string content)
	{
		// Parse details
		if (jToken is JObject detailsObj && detailsObj.TryGetValue("details", out var error))
		{
			var errorString = error.Value<string>() ?? "";
			return new Response<T>([errorString]);
		}

		throw new NotSupportedException($"Received a payload that was not supported:\n{content}");
	}
}
