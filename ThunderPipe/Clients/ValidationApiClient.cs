using ThunderPipe.Utils;

namespace ThunderPipe.Clients;

/// <summary>
/// Client used to call validation API endpoints
/// </summary>
internal sealed class ValidationApiClient : ThunderstoreClient
{
	/// <inheritdoc />
	public ValidationApiClient(RequestBuilder builder, CancellationToken ct)
		: base(builder, ct) { }

	/// <summary>
	/// Validates the icon file at the given path
	/// </summary>
	public async Task<Models.API.ValidateIcon.Response?> ValidateIcon(string path)
	{
		var data = await File.ReadAllBytesAsync(path, CancellationToken);

		var payload = new Models.API.ValidateIcon.Request { Data = Convert.ToBase64String(data) };

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint(API_EXPERIMENTAL + "submission/validate/icon/")
			.WithJSON(payload)
			.Build();

		return await SendRequest<Models.API.ValidateIcon.Response>(request);
	}

	/// <summary>
	/// Validates the manifest file at the given path
	/// </summary>
	public async Task<Models.API.ValidateManifest.Response?> ValidateManifest(
		string path,
		string team
	)
	{
		var data = await File.ReadAllBytesAsync(path, CancellationToken);

		var payload = new Models.API.ValidateManifest.Request
		{
			AuthorName = team,
			Data = Convert.ToBase64String(data),
		};

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint(API_EXPERIMENTAL + "submission/validate/manifest-v1/")
			.WithJSON(payload)
			.Build();

		return await SendRequest<Models.API.ValidateManifest.Response>(request);
	}

	/// <summary>
	/// Validates the README file at the given path
	/// </summary>
	public async Task<Models.API.ValidateReadme.Response?> ValidateReadme(string path)
	{
		var data = await File.ReadAllBytesAsync(path, CancellationToken);

		var payload = new Models.API.ValidateReadme.Request { Data = Convert.ToBase64String(data) };

		var request = Builder
			.Copy()
			.Post()
			.ToEndpoint(API_EXPERIMENTAL + "submission/validate/readme/")
			.WithJSON(payload)
			.Build();

		return await SendRequest<Models.API.ValidateReadme.Response>(request);
	}
}
