using ThunderPipe.Utils;

namespace ThunderPipe.Tests.UnitTests.Utils;

public class UrlHelperTests
{
	public static IEnumerable<object?[]> GetQueryValueData =>
		new List<object?[]>
		{
			new object?[] { "https://thunderstore.io", "user-id", null },
			new object?[] { "https://thunderstore.io?user=0&id=1", "user-id", null },
			new object?[] { "https://thunderstore.io?user-id=baba", "user-id", "baba" },
			new object?[] { "https://thunderstore.io?id=10&id=12", "id", "10,12" },
			// https://stackoverflow.com/a/63455677
			new object?[] { "https://thunderstore.io?ID=10&id=12&Id=a&iD=2", "id", "10,12,a,2" },
			new object?[] { "https://thunderstore.io?IDs=10&id=12&Id3=a&iD69=2", "id", "12" },
		};

	[Theory]
	[MemberData(nameof(GetQueryValueData))]
	public void GetQueryValue(string url, string name, string? expected)
	{
		var actual = UrlHelper.GetQueryValue(url, name);

		Assert.Equal(expected, actual);
	}
}
