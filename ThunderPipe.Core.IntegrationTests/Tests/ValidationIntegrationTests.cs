using ThunderPipe.Core.Services.Implementations;
using Xunit;
using ThunderPipe.Core.IntegrationTests.Infrastructure;

namespace ThunderPipe.Core.IntegrationTests;

/// <summary>Exercise authentication, successful validation and rejected content over HTTP.</summary>
[Collection(ThunderstoreCollection.NAME)]
[Trait("Category", "Integration")]
public sealed class ValidationIntegrationTests
{
	private readonly ThunderstoreFixture _env;
	private readonly FileSystem _fs = new();

	public ValidationIntegrationTests(ThunderstoreFixture env) => _env = env;

	[Fact]
	public async Task Icon_ValidPng_Accepted()
	{
		using var item = new PackageFixture();
		var errors = await _env.Validation.IsIconValid(
			item.IconPath,
			_fs,
			_env.Data.Token,
			TestContext.Current.CancellationToken
		);
		Assert.Empty(errors);
	}

	[Fact]
	public async Task Icon_InvalidBytes_Rejected()
	{
		using var item = new PackageFixture();
		string file = Path.Combine(Path.GetDirectoryName(item.ArchivePath)!, "bad-icon.png");
		await File.WriteAllBytesAsync(file, [1, 2, 3, 4], TestContext.Current.CancellationToken);
		var errors = await _env.Validation.IsIconValid(
			file,
			_fs,
			_env.Data.Token,
			TestContext.Current.CancellationToken
		);
		Assert.NotEmpty(errors);
	}

	[Fact]
	public async Task Manifest_ValidForTeam_Accepted()
	{
		using var item = new PackageFixture();
		var errors = await _env.Validation.IsManifestValid(
			item.ManifestPath,
			_env.Data.Team,
			_fs,
			_env.Data.Token,
			TestContext.Current.CancellationToken
		);
		Assert.Empty(errors);
	}

	[Fact]
	public async Task Manifest_MalformedJson_Rejected()
	{
		using var item = new PackageFixture();
		string file = Path.Combine(Path.GetDirectoryName(item.ArchivePath)!, "bad-manifest.json");
		await File.WriteAllTextAsync(file, "{ invalid json", TestContext.Current.CancellationToken);
		var errors = await _env.Validation.IsManifestValid(
			file,
			_env.Data.Team,
			_fs,
			_env.Data.Token,
			TestContext.Current.CancellationToken
		);
		Assert.NotEmpty(errors);
	}

	[Fact]
	public async Task Readme_ValidMarkdown_Accepted()
	{
		using var item = new PackageFixture();
		var errors = await _env.Validation.IsReadmeValid(
			item.ReadmePath,
			_fs,
			_env.Data.Token,
			TestContext.Current.CancellationToken
		);
		Assert.Empty(errors);
	}

	[Fact]
	public async Task Readme_InvalidUtf8_Rejected()
	{
		using var item = new PackageFixture();
		string file = Path.Combine(Path.GetDirectoryName(item.ArchivePath)!, "bad-readme.md");
		await File.WriteAllBytesAsync(
			file,
			[0xFF, 0xFE, 0x80],
			TestContext.Current.CancellationToken
		);
		var errors = await _env.Validation.IsReadmeValid(
			file,
			_fs,
			_env.Data.Token,
			TestContext.Current.CancellationToken
		);
		Assert.NotEmpty(errors);
	}

	[Fact]
	public async Task Icon_MissingAuthentication_IsRejected()
	{
		using var item = new PackageFixture();
		var errors = await _env.Validation.IsIconValid(
			item.IconPath,
			_fs,
			"not-a-token",
			TestContext.Current.CancellationToken
		);
		Assert.NotEmpty(errors);
	}
}
