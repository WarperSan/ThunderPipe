using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Clients;

/// <summary>
/// Client used to call validation API endpoints
/// </summary>
public sealed class ValidationApiClient : ThunderstoreClient
{
	/// <summary>
	/// Checks if the icon at the given path is valid
	/// </summary>
	public async Task<IReadOnlyCollection<string>> IsIconValid(
		string path,
		IFileSystem fileSystem,
		string token,
		CancellationToken ct = default
	)
	{
		var data = await fileSystem.ReadAllBytesAsync(path, ct);

		var payload = new Models.Web.ValidateIcon.Request { Data = Convert.ToBase64String(data) };
		var request = new RequestBuilder(Builder)
			.Post()
			.WithAuth(token)
			.ToEndpoint("api/experimental/submission/validate/icon/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.Web.ValidateIcon.Response>(request, ct);

		if (response.IsSuccess)
		{
			if (response.Data != null && response.Data.Valid)
				return [];

			throw new InvalidOperationException("Icon was not marked as valid.");
		}

		return response.AllErrors.ToList();
	}

	/// <summary>
	/// Checks if the manifest at the given path is valid for the given team
	/// </summary>
	public async Task<IReadOnlyCollection<string>> IsManifestValid(
		string path,
		Team team,
		IFileSystem fileSystem,
		string token,
		CancellationToken ct = default
	)
	{
		var data = await fileSystem.ReadAllBytesAsync(path, ct);

		var payload = new Models.Web.ValidateManifest.Request
		{
			AuthorName = team,
			Data = Convert.ToBase64String(data),
		};

		var request = new RequestBuilder(Builder)
			.Post()
			.WithAuth(token)
			.ToEndpoint("api/experimental/submission/validate/manifest-v1/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.Web.ValidateManifest.Response>(request, ct);

		if (response.IsSuccess)
		{
			if (response.Data != null && response.Data.Valid)
				return [];

			throw new InvalidOperationException("Manifest was not marked as valid.");
		}

		return response.AllErrors.ToList();
	}

	/// <summary>
	/// Checks if the README at the given path is valid
	/// </summary>
	public async Task<IReadOnlyCollection<string>> IsReadmeValid(
		string path,
		IFileSystem fileSystem,
		string token,
		CancellationToken ct = default
	)
	{
		var data = await fileSystem.ReadAllBytesAsync(path, ct);

		var payload = new Models.Web.ValidateReadme.Request { Data = Convert.ToBase64String(data) };

		var request = new RequestBuilder(Builder)
			.Post()
			.WithAuth(token)
			.ToEndpoint("api/experimental/submission/validate/readme/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.Web.ValidateReadme.Response>(request, ct);

		if (response.IsSuccess)
		{
			if (response.Data != null && response.Data.Valid)
				return [];

			throw new InvalidOperationException("README was not marked as valid.");
		}

		return response.AllErrors.ToList();
	}
}
