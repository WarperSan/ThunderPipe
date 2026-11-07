using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ThunderPipe.Core.IntegrationTests;

internal sealed class PackageFixture : IDisposable
{
	public string Name { get; }
	public string Version { get; } = "1.0.0";
	public string ArchivePath { get; }
	public string IconPath => Path.Combine(AppContext.BaseDirectory, "Fixtures", "icon.png");
	public string ManifestPath { get; }
	public string ReadmePath { get; }

	// Multi-part threshold in Thunderstore is currently 50 MiB. Generate > 50 MiB
	// of incompressible data inside the ZIP so a real two-part S3 upload is required.
	public PackageFixture(bool multipleParts = false)
	{
		Name = "TPIntegration" + Guid.NewGuid().ToString("N")[..16];
		string directory = Path.Combine(Path.GetTempPath(), "thunderpipe-integration", Name);
		Directory.CreateDirectory(directory);
		ArchivePath = Path.Combine(directory, "package.zip");
		ManifestPath = Path.Combine(directory, "manifest.json");
		ReadmePath = Path.Combine(directory, "README.md");
		File.WriteAllText(
			ManifestPath,
			JsonSerializer.Serialize(
				new
				{
					name = Name,
					version_number = Version,
					website_url = "https://example.org",
					description = "ThunderPipe integration test archive",
					dependencies = Array.Empty<string>(),
				}
			)
		);
		File.WriteAllText(ReadmePath, "# ThunderPipe local integration test\n");

		using var zipFile = new FileStream(ArchivePath, FileMode.CreateNew);
		using var zip = new ZipArchive(zipFile, ZipArchiveMode.Create);
		WriteEntry(zip, "manifest.json", File.ReadAllBytes(ManifestPath));
		WriteEntry(zip, "README.md", File.ReadAllBytes(ReadmePath));
		WriteEntry(zip, "icon.png", File.ReadAllBytes(IconPath));
		if (multipleParts)
		{
			using var output = zip.CreateEntry(
					"multipart-payload.bin",
					CompressionLevel.NoCompression
				)
				.Open();
			byte[] chunk = new byte[65536];
			var random = new Random(1242026);
			const long total = 50L * 1024 * 1024 + 1024 * 1024;
			for (long written = 0; written < total; written += chunk.Length)
			{
				random.NextBytes(chunk);
				output.Write(chunk);
			}
		}
	}

	private static void WriteEntry(ZipArchive archive, string name, byte[] content)
	{
		using var output = archive.CreateEntry(name, CompressionLevel.Optimal).Open();
		output.Write(content);
	}

	public static async Task<string> Sha256Async(Stream stream, CancellationToken ct)
	{
		using var sha = SHA256.Create();
		byte[] hash = await sha.ComputeHashAsync(stream, ct);
		return Convert.ToHexString(hash);
	}

	public void Dispose()
	{
		var directory = Path.GetDirectoryName(ArchivePath)!;
		if (Directory.Exists(directory))
			Directory.Delete(directory, true);
	}
}
