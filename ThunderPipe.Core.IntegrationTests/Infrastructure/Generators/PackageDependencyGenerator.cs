using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure.Generators;

/// <summary>
/// Class used to generate instances of <see cref="PackageDependency"/>
/// </summary>
public static class PackageDependencyGenerator
{
	/// <summary>
	/// Creates a <see cref="PackageDependency"/> from the given instance and the given <see cref="Team"/>
	/// </summary>
	public static PackageDependency ChangeTeam(PackageDependency dependency, Team team)
	{
		var name = dependency.Name;
		var version = dependency.Version;

		return new PackageDependency(team, name, version);
	}

	/// <summary>
	/// Creates a <see cref="PackageDependency"/> from the given instance and the given <see cref="PackageName"/>
	/// </summary>
	public static PackageDependency ChangeName(PackageDependency dependency, PackageName name)
	{
		var team = dependency.Team;
		var version = dependency.Version;

		return new PackageDependency(team, name, version);
	}

	/// <summary>
	/// Creates a <see cref="PackageDependency"/> from the given instance and the given <see cref="PackageVersion"/>
	/// </summary>
	public static PackageDependency ChangeVersion(
		PackageDependency dependency,
		PackageVersion version
	)
	{
		var team = dependency.Team;
		var name = dependency.Name;

		return new PackageDependency(team, name, version);
	}
}
