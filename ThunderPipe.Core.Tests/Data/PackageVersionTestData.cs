using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.Tests.Data;

/// <summary>
/// Test data for <see cref="PackageVersion"/> tests
/// </summary>
public class PackageVersionTestData
{
	/// <summary>
	/// List of valid version components
	/// </summary>
	public static TheoryData<int, int, int> ValidVersions =>
		new()
		{
			{ 5, 4, 2121 },
			{ 1, 0, 0 },
			{ 1, 10, 0 },
			{ 111, 0, 0 },
			{ 0, 0, 110 },
		};

	/// <summary>
	/// List of invalid version components
	/// </summary>
	public static TheoryData<int, int, int> InvalidVersions =>
		new()
		{
			{ -1, 0, 0 },
			{ 1, -10, 0 },
			{ 1, 0, -100 },
			{ -1, -2, -3 },
		};

	/// <summary>
	/// List of version strings with fewer than <c>3</c> components
	/// </summary>
	public static TheoryData<string> TooFewParts => ["", "1", "1.0", "1.0-beta", "1.0_0"];

	/// <summary>
	/// List of version strings with more than <c>3</c> components
	/// </summary>
	public static TheoryData<string> TooManyParts =>
		["1.0.0.0", "1.0.0-beta", "12.0.0-alpha", "1.0.0.0.0", "1.0.0-rc.1", "1.0.0+build.123"];

	/// <summary>
	/// List of valid version strings with an invalid <see cref="PackageVersion.Major"/> component
	/// </summary>
	public static TheoryData<string> InvalidMajor =>
		["1a.5.5", "e.5.5", "1e5.5.5", ".5.5", "1_5.5.5"];

	/// <summary>
	/// List of valid version strings with an invalid <see cref="PackageVersion.Minor"/> component
	/// </summary>
	public static TheoryData<string> InvalidMinor =>
		["1.5a.5", "1.e.5", "1.1e5.5", "1..5", "1.5_5.5"];

	/// <summary>
	/// List of valid version strings with an invalid <see cref="PackageVersion.Patch"/> component
	/// </summary>
	public static TheoryData<string> InvalidPatch =>
		["1.5.5a", "1.5.e", "1.5.1e5", "1.5.", "1.5.5_5"];

	/// <summary>
	/// List of valid version strings
	/// </summary>
	public static TheoryData<string> ValidParseVersions =>
		["5.4.2121", "1.0.0", "1.10.0", "111.0.0"];

	/// <summary>
	/// List of invalid version strings
	/// </summary>
	public static TheoryData<string> InvalidParseVersions =>
		[
			"-1.0.0",
			"1.-124678.0",
			"1.0.-621",
			"-1.-2.-3",
			"-2147483648.0.0",
			"0.-2147483648.0",
			"0.0.-2147483648",
		];

	/// <summary>
	/// List of valid version strings with components outside the <see cref="int"/> range
	/// </summary>
	public static TheoryData<string> OutOfRangeVersions =>
		[
			"-2147483649.0.0",
			"0.-2147483649.0",
			"0.0.-2147483649",
			"2147483649.0.0",
			"0.2147483649.0",
			"0.0.2147483649",
		];

	/// <summary>
	/// List of invalid version strings
	/// </summary>
	public static TheoryData<string> InvalidVersionStrings => ["1.0", "1.0.0.0", "", "abc.def.ghi"];
}
