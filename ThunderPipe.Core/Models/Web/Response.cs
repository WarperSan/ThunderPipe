using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThunderPipe.Core.Models.Web;

/// <summary>
/// Wrapper to handle <see cref="HttpResponseMessage"/> better
/// </summary>
public sealed class Response<T>
	where T : class
{
	/// <summary>
	/// Category used when the error is not related to a specific field
	/// </summary>
	public const string GLOBAL_ERRORS = "global";

	private Response(T data)
	{
		IsSuccess = true;
		Data = data;
		Errors = new Dictionary<string, IEnumerable<string>>();
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
	/// All errors returned
	/// </summary>
	public IEnumerable<string> AllErrors => Errors.SelectMany(e => e.Value);

	/// <summary>
	/// Logs the errors of this instance
	/// </summary>
	public void LogErrors(ILogger? logger)
	{
		if (logger == null)
			return;

		if (Errors.Count == 0)
			return;

		var output = new StringBuilder();
		output.AppendLine("Errors:");

		foreach (var error in Errors)
		{
			output.AppendLine($"- [{error.Key}]:");

			foreach (var errorValue in error.Value)
				output.AppendLine($"    {errorValue}");
		}

		logger.LogError("{Errors}", output.ToString());
	}

	/// <summary>
	/// Throws an exception if this response is not a success
	/// </summary>
	public void EnsureSuccess(out T data)
	{
		if (!IsSuccess)
			throw new InvalidOperationException("Expected a successful response.");

		data =
			Data
			?? throw new NullReferenceException(
				"The response was a success, but the data did not load properly."
			);
	}

	/// <summary>
	/// Creates a new instance of <see cref="Response{T}"/> from the given response
	/// </summary>
	public static Response<T> CreateResponse(HttpResponseMessage response, string content)
	{
		if (response.IsSuccessStatusCode)
			return HandleSuccess(content);

		var status = response.StatusCode;

		var jToken = JToken.Parse(content);

		if (status == HttpStatusCode.BadRequest)
			return HandleBadRequest(jToken);

		return HandleError(jToken);
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

	private static Response<T> HandleSuccess(string content)
	{
		var data = ParseJson<T>(content);

		return new Response<T>(data);
	}

	private static Response<T> HandleBadRequest(JToken jToken)
	{
		switch (jToken)
		{
			case JObject jObject:
				return ParseObjectError(jObject);
			case JArray jArray:
			{
				// if is array, parse all errors as global
				var errors = jArray.Values<string>().OfType<string>();

				return new Response<T>(
					new Dictionary<string, IEnumerable<string>>() { [GLOBAL_ERRORS] = errors }
				);
			}
			default:
				throw new InvalidOperationException(
					$"Cannot parse a '{HttpStatusCode.BadRequest:D}' response when it's not an array or an object."
				);
		}
	}

	private static Response<T> HandleError(JToken jToken)
	{
		// Parse details
		if (jToken is JObject detailsObj && detailsObj.TryGetValue("detail", out var error))
		{
			var errorString = error.Value<string>() ?? "";

			return new Response<T>(
				new Dictionary<string, IEnumerable<string>>() { [GLOBAL_ERRORS] = [errorString] }
			);
		}

		throw new NotSupportedException($"Received a payload that was not supported:\n{jToken}");
	}

	private static Response<T> ParseObjectError(JToken jToken)
	{
		var tokensToProcess = new Queue<JToken>();
		var allErrors = new Dictionary<string, List<string>>();

		tokensToProcess.Enqueue(jToken);

		while (tokensToProcess.Count > 0)
		{
			var token = tokensToProcess.Dequeue();

			switch (token)
			{
				case JObject jObject:
				{
					foreach (var property in jObject.Properties())
						tokensToProcess.Enqueue(property.Value);
					break;
				}
				case JArray jArray:
					foreach (var item in jArray)
						tokensToProcess.Enqueue(item);
					break;
				default:
					var key = token.Path;

					if (token.Parent != null && token.Parent.Type == JTokenType.Array)
						key = token.Parent.Path;

					if (!allErrors.ContainsKey(key))
						allErrors.Add(key, []);

					var error = token.Value<string>();

					if (!string.IsNullOrEmpty(error))
						allErrors[key].Add(error);
					break;
			}
		}

		var readOnlyErrors = new Dictionary<string, IEnumerable<string>>();

		foreach (var error in allErrors)
			readOnlyErrors[error.Key] = error.Value;

		return new Response<T>(readOnlyErrors);
	}
}
