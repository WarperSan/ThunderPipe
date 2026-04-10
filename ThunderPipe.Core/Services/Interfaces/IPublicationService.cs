using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Core.Services.Interfaces;

/// <summary>
/// Handles the publication of package content
/// </summary>
public interface IPublicationService
{
	/// <summary>
	/// Publishes the given package
	/// </summary>
	public Task<Package> PublishPackage(
		string file,
		Team team,
		IEnumerable<Community> communities,
		IDictionary<Community, List<Category>> categories,
		bool hasNsfw,
		string token,
		CancellationToken cancellationToken
	);
}
