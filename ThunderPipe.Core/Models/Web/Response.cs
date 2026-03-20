namespace ThunderPipe.Core.Models.Web;

/// <summary>
/// Wrapper to handle <see cref="HttpResponseMessage"/> better
/// </summary>
internal sealed class Response<T>
	where T : class
{
	/// <summary>
	/// JSON content of the response if success
	/// </summary>
	public T? Data { get; private set; }
}
