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
	) { }
}
