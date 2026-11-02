using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>
/// Class used to generate instances of <see cref="Community"/>
/// </summary>
public static class CommunityGenerator
{
	/// <summary>
	/// Generates a <see cref="Community"/> that is missing from the service
	/// </summary>
	public static Community GenerateMissing()
	{
		return $"missing-community-{Guid.NewGuid():N}";
	}
}
