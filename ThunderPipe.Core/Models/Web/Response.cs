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
		Errors = new Dictionary<string, IEnumerable<string>>() { ["global"] = errors };
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
		{
			// Parse normally
			var data = ParseJson<T>(content);

			return new Response<T>(data);
		}

		var jObject = JObject.Parse(content);

		if (status == HttpStatusCode.BadRequest)
		{
			if (jObject.Type == JTokenType.Object)
			{
				// if object, parse every error as field specific
				var allErrors = new Dictionary<string, IEnumerable<string>>();

				foreach (var property in jObject.Properties())
				{
					var category = property.Name;
					var errors = property.Values<string>().OfType<string>();

					allErrors[category] = errors;
				}

				return new Response<T>(allErrors);
			}

			if (jObject.Type == JTokenType.Array)
			{
				// if array, parse all errors as global
				var errors = jObject.Values<string>().OfType<string>();

				return new Response<T>(errors);
			}

			throw new InvalidOperationException(
				$"Cannot parse a '{status:D}' response when it's not an array or an object."
			);
		}

		// Parse details
		if (jObject.TryGetValue("details", out var error))
		{
			var errorString = error.Value<string>() ?? "";
			return new Response<T>([errorString]);
		}

		throw new NotSupportedException($"Received a payload that was not supported:\n{content}");
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
