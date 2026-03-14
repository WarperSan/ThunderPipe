using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Core.Services.Interfaces;

/// <summary>
/// Handles the validation of package content
/// </summary>
public interface IValidationService
{
	/// <summary>
	/// Validates the given files of a package
	/// </summary>
	public Task<ICollection<string>> ValidatePackage(
		Team team,
		string iconPath,
		string manifestPath,
		string readmePath,
		string token,
		CancellationToken cancellationToken
	);
}
