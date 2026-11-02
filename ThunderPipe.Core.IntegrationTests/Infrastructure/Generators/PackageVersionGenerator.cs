using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>
/// Class used to generate instances of <see cref="PackageVersion"/>
/// </summary>
public static class PackageVersionGenerator
{
	/// <summary>
	/// Generates a <see cref="PackageVersion"/> greater than the given <see cref="PackageVersion"/>
	/// </summary>
	public static PackageVersion GenerateGreater(PackageVersion version)
	{
		const int MAXIMUM_MAJOR = int.MaxValue;
		const int MAXIMUM_MINOR = int.MaxValue;
		const int MAXIMUM_PATCH = int.MaxValue;

		var major = version.Major;
		var minor = version.Minor;
		var patch = version.Patch;

		// ReSharper disable once ConvertIfStatementToSwitchStatement
		if (major == MAXIMUM_MAJOR && minor == MAXIMUM_MINOR && patch == MAXIMUM_PATCH)
			throw new ArgumentException("Version is already at its maximum.", nameof(version));

		if (major < MAXIMUM_MAJOR)
		{
			major = Random.Shared.Next(major + 1, MAXIMUM_MAJOR);
		}
		else if (minor < MAXIMUM_MINOR)
		{
			minor = Random.Shared.Next(minor + 1, MAXIMUM_MINOR);
		}
		else
		{
			patch = Random.Shared.Next(patch + 1, MAXIMUM_PATCH);
		}

		return new PackageVersion(major, minor, patch);
	}
}
