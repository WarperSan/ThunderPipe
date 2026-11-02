using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>
/// Class used to generate instances of <see cref="Team"/>
/// </summary>
public static class TeamGenerator
{
	/// <summary>
	/// Generates a <see cref="Team"/> that is missing from the service
	/// </summary>
	public static Team GenerateMissing()
	{
		return $"Missing_Team_{Guid.NewGuid():N}";
	}
}
