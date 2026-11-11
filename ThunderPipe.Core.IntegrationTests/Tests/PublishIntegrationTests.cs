using System.Net;
using System.Security.Cryptography;
using ThunderPipe.Core.DTOs.MultipartUpload;
using ThunderPipe.Core.Models;
using ThunderPipe.Core.Services.Implementations;
using Xunit;
using ThunderPipe.Core.IntegrationTests.Infrastructure;

namespace ThunderPipe.Core.IntegrationTests;

/// <summary>
/// Full upload/submit/download round trips, not response-schema snapshots or mocked HTTP.
/// Multipart tests deliberately use PublishApiClient.UploadParts to detect shared-stream races.
/// </summary>
[Collection(ThunderstoreCollection.NAME)]
[Trait("Category", "Integration")]
public sealed class PublishIntegrationTests
{
	private readonly ThunderstoreFixture _env;
	private readonly FileSystem _fs = new();

	public PublishIntegrationTests(ThunderstoreFixture env) => _env = env;

	[Fact]
	public async Task Initiate_ThenAbort_InvalidatesPresignedPart()
	{
		using var item = new PackageFixture();
		var ct = TestContext.Current.CancellationToken;
		UploadSession session = await _env.Publish.InitiateMultipartUpload(
			item.ArchivePath,
			_fs,
			_env.Data.Token,
			ct
		);
		Assert.True(Guid.TryParse(session.UUID, out _));
		Assert.NotEmpty(session.Parts);
		var first = session.Parts.OrderBy(p => p.Offset).First();
		Assert.Equal(0L, first.Offset);
		Assert.True(first.Size > 0);
		Assert.True(first.UploadURL.IsAbsoluteUri);
		await _env.Publish.AbortMultipartUpload(session.UUID, _env.Data.Token, ct);

		// The public Abort method ignores the HTTP status; verify the side effect
		// against the actual S3-compatible service rather than trusting a void return.
		using var request = new HttpRequestMessage(HttpMethod.Put, first.UploadURL)
		{
			Content = new ByteArrayContent(new byte[first.Size]),
		};
		request.Content.Headers.ContentLength = first.Size;
		using var response = await _env.Http.SendAsync(request, ct);
		Assert.False(response.IsSuccessStatusCode);
	}

	[Fact]
	public async Task Initiate_InvalidBearerToken_IsRejected()
	{
		using var item = new PackageFixture();
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			_env.Publish.InitiateMultipartUpload(
				item.ArchivePath,
				_fs,
				"not-a-token",
				TestContext.Current.CancellationToken
			)
		);
	}

	[Fact]
	public async Task Finish_NonexistentSession_ReturnsFalse()
	{
		bool completed = await _env.Publish.FinishMultipartUpload(
			Guid.NewGuid().ToString(),
			[new UploadPart { Id = 1, ETag = "\"does-not-exist\"" }],
			_env.Data.Token,
			TestContext.Current.CancellationToken
		);
		Assert.False(completed);
	}

	[Fact]
	public async Task Publish_OnePart_Submit_VerifyDownloadedBytesAndVersion()
	{
		using var item = new PackageFixture();
		await PublishAndVerify(item, expectedParts: 1, TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task Publish_TwoParts_Submit_VerifyDownloadedBytesAndVersion()
	{
		using var item = new PackageFixture(multipleParts: true);
		Assert.True(new FileInfo(item.ArchivePath).Length > 50L * 1024 * 1024);
		await PublishAndVerify(item, expectedParts: 2, TestContext.Current.CancellationToken);
	}

	private static void RequireLocal(Uri uri)
	{
		if (uri.Scheme is not ("http" or "https")
			|| !(uri.IsLoopback || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)))
			throw new InvalidOperationException($"Refusing non-local upload/download URL: {uri.Host}");
	}

	private async Task<HttpResponseMessage> DownloadLocalRedirectsAsync(
		Uri url,
		CancellationToken ct
	)
	{
		// Thunderstore's published download URL is a Django endpoint that issues
		// an HTTP redirect to local object storage. Every redirect is checked
		// before following it; the HTTP handler never auto-follows redirects.
		for (int hop = 0; hop < 6; hop++)
		{
			RequireLocal(url);
			var response = await _env.Http.GetAsync(
				url,
				HttpCompletionOption.ResponseHeadersRead,
				ct
			);
			if (
				response.StatusCode
				is HttpStatusCode.MovedPermanently
					or HttpStatusCode.Redirect
					or HttpStatusCode.SeeOther
					or HttpStatusCode.TemporaryRedirect
					or HttpStatusCode.PermanentRedirect
			)
			{
				var location =
					response.Headers.Location
					?? throw new InvalidOperationException(
						"Download redirect has no Location header."
					);
				Uri next = location.IsAbsoluteUri ? location : new Uri(url, location);
				response.Dispose();
				RequireLocal(next);
				url = next;
				continue;
			}
			return response;
		}
		throw new InvalidOperationException("Too many download redirects.");
	}

	private async Task PublishAndVerify(
		PackageFixture item,
		int expectedParts,
		CancellationToken ct
	)
	{
		UploadSession session = await _env.Publish.InitiateMultipartUpload(
			item.ArchivePath,
			_fs,
			_env.Data.Token,
			ct
		);
		Assert.True(Guid.TryParse(session.UUID, out _));
		var descriptors = session.Parts.OrderBy(p => p.Offset).ToArray();
		Assert.Equal(expectedParts, descriptors.Length);
		Assert.Equal(0L, descriptors[0].Offset);
		long offset = 0;
		foreach (var part in descriptors)
		{
			Assert.Equal(offset, part.Offset);
			Assert.True(part.Size > 0);
			RequireLocal(part.UploadURL);
			offset += part.Size;
		}
		Assert.Equal(new FileInfo(item.ArchivePath).Length, offset);

		IReadOnlyCollection<UploadPart> uploaded = await _env.Publish.UploadParts(
			item.ArchivePath,
			descriptors,
			_fs,
			ct
		);
		Assert.Equal(descriptors.Length, uploaded.Count);
		Assert.Equal(
			descriptors.Select(p => p.Id).OrderBy(x => x),
			uploaded.Select(p => p.Id).OrderBy(x => x)
		);
		Assert.All(uploaded, part => Assert.False(string.IsNullOrWhiteSpace(part.ETag)));

		bool finished = await _env.Publish.FinishMultipartUpload(
			session.UUID,
			uploaded,
			_env.Data.Token,
			ct
		);
		Assert.True(finished);

		// SubmitPackage is deliberately exercised, not replaced by direct raw HTTP.
		Community community = _env.Data.Community;
		// Thunderstore accepts publishing without assigning categories.
		Package published = await _env.Publish.SubmitPackage(
			_env.Data.Team,
			[community],
			new Dictionary<Community, IEnumerable<Category>> { [community] = [] },
			false,
			session.UUID,
			_env.Data.Token,
			ct
		);
		Assert.Equal(item.Name, (string)published.Name);
		Assert.Equal(item.Version, (string)published.Version);
		Assert.True(published.DownloadURL.IsAbsoluteUri);
		RequireLocal(published.DownloadURL);

		// A successful upload is not enough: the published archive must contain
		// exactly the same bytes, including every multipart segment.
		using var download = await DownloadLocalRedirectsAsync(published.DownloadURL, ct);
		download.EnsureSuccessStatusCode();
		await using var downloadedStream = await download.Content.ReadAsStreamAsync(ct);
		await using var sourceStream = File.OpenRead(item.ArchivePath);
		string expectedHash = await PackageFixture.Sha256Async(sourceStream, ct);
		string actualHash = await PackageFixture.Sha256Async(downloadedStream, ct);
		Assert.Equal(expectedHash, actualHash);

		// Verify publicly visible state via another ThunderPipe client.
		PackageVersion latest = await _env.Packages.GetVersion(_env.Data.Team, item.Name, ct);
		Assert.Equal(item.Version, (string)latest);
		var missing = await _env.Dependencies.GetMissing(
			[new PackageDependency($"{_env.Data.Team}-{item.Name}-{item.Version}")],
			ct
		);
		Assert.Empty(missing);
	}
}
