using System.Web;

namespace ThunderPipe.Utils;

/// <summary>
/// Class that holds useful methods for URLs
/// </summary>
internal static class UrlHelper
{
	/// <summary>
	/// Gets the query value at the given name
	/// </summary>
	public static string? GetQueryValue(string url, string name)
	{
		var uri = new Uri(url);
		var query = HttpUtility.ParseQueryString(uri.Query);
		return query.Get(name);
	}
}
