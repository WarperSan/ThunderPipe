using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Services.Implementations;

public sealed class ValidationService : IValidationService
{
	private readonly ValidationApiClient _client;
	private readonly IFileSystem _fileSystem;

	public ValidationService(RequestBuilder builder, IFileSystem fileSystem, ILogger logger)
	{
		_client = new ValidationApiClient();
		_client.Builder = builder;
		_client.Logger = logger;

		_fileSystem = fileSystem;
	}

	/// <inheritdoc />
	public async Task<ICollection<string>> ValidatePackage(
		Team team,
		string iconPath,
		string manifestPath,
		string readmePath,
		string token,
		CancellationToken cancellationToken
	)
	{
		var errors = new List<string>();

		errors.AddRange(await _client.IsIconValid(iconPath, _fileSystem, token, cancellationToken));

		errors.AddRange(
			await _client.IsManifestValid(manifestPath, team, _fileSystem, token, cancellationToken)
		);

		// TODO: Check why this returns a 500
		//errors.AddRange(await _client.IsReadmeValid(readmePath, _fileSystem, token, cancellationToken));

		return errors;
	}
}
