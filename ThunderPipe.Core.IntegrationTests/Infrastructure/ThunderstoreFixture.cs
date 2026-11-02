using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.IntegrationTests.Infrastructure;

/// <summary>
/// Class holding information shared across all tests
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ThunderstoreFixture : IAsyncLifetime
{
	private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(60) };

	/// <summary>
	/// Instance of <see cref="CategoryApiClient"/>
	/// </summary>
	public CategoryApiClient Categories { get; private set; } = null!;

	/// <summary>
	/// Instance of <see cref="CommunityApiClient"/>
	/// </summary>
	public CommunityApiClient Communities { get; private set; } = null!;

	/// <summary>
	/// Instance of <see cref="DependencyApiClient"/>
	/// </summary>
	public DependencyApiClient Dependencies { get; private set; } = null!;

	/// <summary>
	/// Instance of <see cref="PackageApiClient"/>
	/// </summary>
	public PackageApiClient Packages { get; private set; } = null!;

	/// <summary>
	/// Instance of <see cref="PublishApiClient"/>
	/// </summary>
	public PublishApiClient Publish { get; private set; } = null!;

	/// <summary>
	/// Instance of <see cref="ValidationApiClient"/>
	/// </summary>
	public ValidationApiClient Validation { get; private set; } = null!;

	/// <inheritdoc />
	public ValueTask InitializeAsync()
	{
		var builder = new RequestBuilder().ToUri(Settings.BaseUri);

		Categories = new CategoryApiClient(_http, builder);
		Communities = new CommunityApiClient(_http, builder);
		Dependencies = new DependencyApiClient(_http, builder);
		Packages = new PackageApiClient(_http, builder);
		Publish = new PublishApiClient(_http, builder);
		Validation = new ValidationApiClient(_http, builder);

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public ValueTask DisposeAsync()
	{
		_http.Dispose();

		return ValueTask.CompletedTask;
	}
}
