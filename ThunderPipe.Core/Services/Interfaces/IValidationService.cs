using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Core.Services.Interfaces;

/// <summary>
/// Handles the validation of package content
/// </summary>
public interface IValidationService
{
	public Task<ICollection<string>> ValidatePackage(
		Team team,
		string iconPath,
		string manifestPath,
		string readmePath,
		CancellationToken cancellationToken
	);
}
