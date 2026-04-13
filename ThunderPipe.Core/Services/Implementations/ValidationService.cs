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

		try
		{
			var iconErrors = await _client.IsIconValid(
				iconPath,
				_fileSystem,
				token,
				cancellationToken
			);

			var errorPrefix = $"['{iconPath}']";

			errors.AddRange(iconErrors.Select(error => $"{errorPrefix} {error}"));
		}
		catch (Exception e)
		{
			errors.Add(e.Message);
		}

		try
		{
			var manifestErrors = await _client.IsManifestValid(
				manifestPath,
				team,
				_fileSystem,
				token,
				cancellationToken
			);

			var errorPrefix = $"['{manifestPath}']";

			errors.AddRange(manifestErrors.Select(error => $"{errorPrefix} {error}"));
		}
		catch (Exception e)
		{
			errors.Add(e.Message);
		}

		try
		{
			// TODO: Check why this returns a 500
			/*var readmeErrors = await _client.IsReadmeValid(
				readmePath,
				_fileSystem,
				token,
				cancellationToken
			);

			var errorPrefix = $"['{readmePath}']";

			errors.AddRange(readmeErrors.Select(error => $"{errorPrefix} {error}"));*/
		}
		catch (Exception e)
		{
			errors.Add(e.Message);
		}

		return errors;
	}
}
