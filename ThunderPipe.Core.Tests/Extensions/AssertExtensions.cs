using System.Collections;

namespace Xunit;

// ReSharper disable once ConvertToStaticClass
// ReSharper disable once ClassNeverInstantiated.Global
public partial class Assert
{
	/// <summary>
	/// Asserts that expected items are missing from actual and counts match.
	/// </summary>
	public static void Missing<T>(IEnumerable<T> expected, IEnumerable<T> actual)
	{
		Equal(expected.Count(), actual.Count());
		Contains(expected, actual);
	}

	/// <summary>
	/// Asserts that expected items are contained in actual.
	/// </summary>
	public static void Contains<T>(IEnumerable<T> expected, IEnumerable<T> actual)
	{
		foreach (var item in expected)
			Contains(item, actual);
	}

	/// <inheritdoc cref="Assert.Single(IEnumerable, object)"/>
	public static void Single<T>(IEnumerable<T> collection, T expected)
	{
		Single((IEnumerable)collection, expected);
	}
}
