using ThunderPipe.Core.Models;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure;

/// <summary>
/// Class holding all the environment variables
/// </summary>
public static class Settings
{
	/// <summary>
	/// Gets the variable at the given name
	/// </summary>
	private static string GetRequired(string name)
	{
		var variable = Environment.GetEnvironmentVariable(name);

		if (variable == null)
			throw new InvalidOperationException(
				$"No value was assigned to the required variable '{name}'."
			);

		return variable;
	}

	/// <summary>
	/// Uri at which the service is located
	/// </summary>
	public static readonly Uri BaseUri = new(GetRequired("THUNDERSTORE_URL"));

	/// <summary>
	/// Community confirmed to exist in the service
	/// </summary>
	public static readonly Community ExistingCommunity = GetRequired("THUNDERSTORE_COMMUNITY");

	/// <summary>
	/// Package confirmed to exist in the service
	/// </summary>
	public static readonly PackageDependency ExistingPackage = GetRequired("THUNDERSTORE_PACKAGE");
}
