using ThunderPipe.Services.Interfaces;
using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call validation API endpoints
/// </summary>
internal sealed class ValidationApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public ValidationApiClient(RequestBuilder builder, HttpClient client, CancellationToken ct)
		: base(builder, client, ct) { }

	/// <summary>
	/// Checks if the icon at the given path is valid
	/// </summary>
	public async Task<ICollection<string>> IsIconValid(string path, IFileSystem fileSystem)
	{
		var data = await fileSystem.ReadAllBytesAsync(path, CancellationToken);

		var payload = new Models.API.ValidateIcon.Request { Data = Convert.ToBase64String(data) };
		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint("api/experimental/submission/validate/icon/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.API.ValidateIcon.Response>(request);

		var errors = new List<string>();

		if (response.DataErrors != null)
			errors.AddRange(response.DataErrors);

		if (response.ValidationErrors != null)
			errors.AddRange(response.ValidationErrors);

		if (errors.Count == 0 && response.Valid is null or false)
			throw new InvalidOperationException("Icon was not marked as valid.");

		return errors;
	}

	/// <summary>
	/// Checks if the manifest at the given path is valid for the given team
	/// </summary>
	public async Task<ICollection<string>> IsManifestValid(
		string path,
		string team,
		IFileSystem fileSystem
	)
	{
		var data = await fileSystem.ReadAllBytesAsync(path, CancellationToken);

		var payload = new Models.API.ValidateManifest.Request
		{
			AuthorName = team,
			Data = Convert.ToBase64String(data),
		};

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint("api/experimental/submission/validate/manifest-v1/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.API.ValidateManifest.Response>(request);

		var errors = new List<string>();

		if (response.DataErrors != null)
			errors.AddRange(response.DataErrors);

		if (response.ValidationErrors != null)
			errors.AddRange(response.ValidationErrors);

		if (errors.Count == 0 && response.Valid is null or false)
			throw new InvalidOperationException("Manifest was not marked as valid.");

		return errors;
	}

	/// <summary>
	/// Checks if the README at the given path is valid
	/// </summary>
	public async Task<ICollection<string>> IsReadmeValid(string path, IFileSystem fileSystem)
	{
		var data = await fileSystem.ReadAllBytesAsync(path, CancellationToken);

		var payload = new Models.API.ValidateReadme.Request { Data = Convert.ToBase64String(data) };

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint("api/experimental/submission/validate/readme/")
			.WithJSON(payload)
			.Build();

		var response = await SendRequest<Models.API.ValidateReadme.Response>(request);

		var errors = new List<string>();

		if (response.DataErrors != null)
			errors.AddRange(response.DataErrors);

		if (response.ValidationErrors != null)
			errors.AddRange(response.ValidationErrors);

		if (errors.Count == 0 && response.Valid is null or false)
			throw new InvalidOperationException("README was not marked as valid.");

		return errors;
	}
}
