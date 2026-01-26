using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Utils;

public class UrlHelperTests
{
	[Theory]
	[InlineData("https://thunderstore.io?user-id=baba", "user-id", "baba")]
	[InlineData("https://thunderstore.io?id=10&id=12", "id", "10,12")]
	// https://stackoverflow.com/a/63455677
	[InlineData("https://thunderstore.io?ID=10&id=12&Id=a&iD=2", "id", "10,12,a,2")]
	[InlineData("https://thunderstore.io?IDs=10&id=12&Id3=a&iD69=2", "id", "12")]
	public void GetQueryValue_WhenHasQueryValue_ReturnValue(
		string url,
		string name,
		string expected
	)
	{
		var actual = UrlHelper.GetQueryValue(url, name);
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("https://thunderstore.io", "user-id")]
	[InlineData("https://thunderstore.io?user=0&id=1", "user-id")]
	public void GetQueryValue_WhenQueryValueNotFound_ReturnNull(string url, string name)
	{
		var actual = UrlHelper.GetQueryValue(url, name);
		Assert.Null(actual);
	}
}
