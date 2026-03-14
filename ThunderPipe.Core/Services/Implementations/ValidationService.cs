using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Interfaces;

namespace ThunderPipe.Core.Services.Implementations;

public sealed class ValidationService : IValidationService
{
	private readonly ValidationApiClient _client;
	private readonly IFileSystem _fileSystem;

	public ValidationService(ValidationApiClient client, IFileSystem fileSystem)
	{
		_client = client;
		_fileSystem = fileSystem;
	}

	/// <inheritdoc />
	public async Task<ICollection<string>> ValidatePackage(
		Team team,
		string iconPath,
		string manifestPath,
		string readmePath,
		CancellationToken cancellationToken
	)
	{
		var errors = new List<string>();

		errors.AddRange(await _client.IsIconValid(iconPath, _fileSystem, cancellationToken));

		errors.AddRange(
			await _client.IsManifestValid(manifestPath, team, _fileSystem, cancellationToken)
		);

		// TODO: Check why this returns a 500
		//errors.AddRange(await client.IsReadmeValid(readmePath, _fileSystem, cancellationToken));

		return errors;
	}
}
