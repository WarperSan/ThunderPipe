using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using ThunderPipe.Core.Models.API;

namespace ThunderPipe.MSBuild.Tasks;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ThunderPipePack : Task
{
	[Required]
	public required string Name { get; set; }

	[Required]
	public required string Version { get; set; }

	public ITaskItem[]? Dependencies { get; set; }

	public string? TemporaryDir { get; set; }
	public string[]? PackageFiles { get; set; }
	public ITaskItem[]? Files { get; set; }

	[Output]
	public required string Output { get; set; }

	/// <inheritdoc />
	public override bool Execute()
	{
		var tempDir = CreateTemporaryDirectory(TemporaryDir);

		CopyPackageFiles(PackageFiles ?? [], tempDir);
		CopyFiles(Files ?? [], tempDir);

		var packageManifest = new PackageManifest
		{
			Name = Name,
			Version = Version,
			Dependencies = ParseDependencies(Dependencies ?? []),
		};

		WritePackageManifest(packageManifest, tempDir);

		// TODO: Validate package

		ZipFile.CreateFromDirectory(tempDir, Output);
		Directory.Delete(tempDir, true);

		return true;
	}

	private static string CreateTemporaryDirectory(string? temporaryDir)
	{
		DirectoryInfo directory;

		if (string.IsNullOrWhiteSpace(temporaryDir))
			directory = Directory.CreateTempSubdirectory();
		else
			directory = Directory.CreateDirectory(temporaryDir);

		return directory.FullName;
	}

	private static void CopyPackageFiles(string[] files, string destination)
	{
		foreach (var file in files)
		{
			var outputFile = Path.Combine(destination, Path.GetFileName(file));
			File.Copy(file, outputFile, true);
		}
	}

	private static void CopyFiles(ITaskItem[] files, string destination)
	{
		foreach (var file in files)
		{
			var destinationFolder = Path.Combine(destination, ResolveFilePath(file));

			if (!Directory.Exists(destinationFolder))
				Directory.CreateDirectory(destinationFolder);

			var targetFile = file.ItemSpec;
			var outputFile = Path.Combine(destinationFolder, targetFile);

			File.Copy(targetFile, outputFile, true);
		}
	}

	private static string ResolveFilePath(ITaskItem file)
	{
		const string PLUGINS_FOLDER = "plugins/";
		var destination = file.GetMetadata("Destination").Trim();

		if (string.IsNullOrEmpty(destination))
			destination = PLUGINS_FOLDER;

		if (destination == ".")
			destination = PLUGINS_FOLDER;

		if (destination.StartsWith('/'))
			destination = PLUGINS_FOLDER + destination[1..];

		if (!destination.EndsWith('/'))
			destination += '/';

		return destination;
	}

	private static PackageDependency[] ParseDependencies(ITaskItem[] dependencies)
	{
		var resolvedDependencies = new PackageDependency[dependencies.Length];

		for (var i = 0; i < resolvedDependencies.Length; i++)
		{
			var dependency = dependencies[i].ItemSpec ?? "";

			resolvedDependencies[i] = dependency;
		}

		return resolvedDependencies;
	}

	private static void WritePackageManifest(PackageManifest packageManifest, string destination)
	{
		var outputFile = Path.Combine(destination, "manifest.json");

		var jsonContent = JsonConvert.SerializeObject(packageManifest, Formatting.Indented);

		File.WriteAllText(outputFile, jsonContent);
	}
}
