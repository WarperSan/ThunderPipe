using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Core.Services.Interfaces;

/// <summary>
/// Handles the creation of package artifacts
/// </summary>
public interface ICreationService
{
	/// <summary>
	/// Creates a `manifest.json` file in the given folder
	/// </summary>
	public Task CreateManifest(
		PackageManifest manifest,
		string destination,
		CancellationToken cancellationToken
	);
}
