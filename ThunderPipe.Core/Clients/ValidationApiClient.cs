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
	public async Task<ICollection<string>> IsIconValid(
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

		var errors = new List<string>();

		if (response.DataErrors != null)
			errors.AddRange(response.DataErrors);

		if (response.ValidationErrors != null)
			errors.AddRange(response.ValidationErrors);

		if (errors.Count == 0 && response.Valid is not true)
			throw new InvalidOperationException("Icon was not marked as valid.");

		if (errors.Count > 0 && response.Valid is true)
			throw new InvalidOperationException("Icon has errors, but was marked as valid.");

		return errors;
	}

	/// <summary>
	/// Checks if the manifest at the given path is valid for the given team
	/// </summary>
	public async Task<ICollection<string>> IsManifestValid(
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

		var errors = new List<string>();

		if (response.DataErrors != null)
			errors.AddRange(response.DataErrors);

		if (response.ValidationErrors != null)
			errors.AddRange(response.ValidationErrors);

		if (response.NamespaceErrors != null)
			errors.AddRange(response.NamespaceErrors);

		if (errors.Count == 0 && response.Valid is not true)
			throw new InvalidOperationException("Manifest was not marked as valid.");

		if (errors.Count > 0 && response.Valid is true)
			throw new InvalidOperationException("Manifest has errors, but was marked as valid.");

		return errors;
	}

	/// <summary>
	/// Checks if the README at the given path is valid
	/// </summary>
	public async Task<ICollection<string>> IsReadmeValid(
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

		var errors = new List<string>();

		if (response.DataErrors != null)
			errors.AddRange(response.DataErrors);

		if (response.ValidationErrors != null)
			errors.AddRange(response.ValidationErrors);

		if (errors.Count == 0 && response.Valid is not true)
			throw new InvalidOperationException("README was not marked as valid.");

		if (errors.Count > 0 && response.Valid is true)
			throw new InvalidOperationException("README has errors, but was marked as valid.");

		return errors;
	}
}
