using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Core.Utils;
using ThunderPipe.MSBuild.Tasks.Helpers;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Task = Microsoft.Build.Utilities.Task;

namespace ThunderPipe.MSBuild.Tasks;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ThunderPipePack : Task
{
	[Required]
	public required string Token { get; set; }

	[Required]
	public required string Team { get; set; }

	[Required]
	public required string Name { get; set; }

	[Required]
	public required string Version { get; set; }

	public string? Description { get; set; }
	public string? Website { get; set; }
	public ITaskItem[]? Dependencies { get; set; }
	public string? TemporaryDir { get; set; }
	public string[]? PackageFiles { get; set; }
	public ITaskItem[]? Files { get; set; }
	public string? Host { get; set; }

	[Output]
	public string? Output { get; set; }

	/// <inheritdoc />
	public override bool Execute()
	{
		var builder = new RequestBuilder();
		var logger = new MSBuildLogger(Log);
		var creationService = new CreationService(new FileSystem(), logger);

		if (!string.IsNullOrEmpty(Host))
			builder.ToUri(new Uri(Host));

		var validationService = new ValidationService(builder, new FileSystem(), logger);

		var tempDir = CreateTemporaryDirectory(TemporaryDir);

		CopyPackageFiles(PackageFiles ?? [], tempDir);
		CopyFiles(Files ?? [], tempDir);

		var packageManifest = new PackageManifest
		{
			Name = Name,
			Version = Version,
			Description = Description ?? "",
			Website = Website ?? "",
			Dependencies =
				Dependencies?.Select(d => (PackageDependency)(d.ItemSpec ?? "")).ToArray() ?? [],
		};

		creationService
			.CreateManifest(packageManifest, tempDir, CancellationToken.None)
			.GetAwaiter()
			.GetResult();

		var isPackageValid = ValidatePackage(validationService, logger, Team, Token, tempDir)
			.GetAwaiter()
			.GetResult();

		if (!isPackageValid)
			return false;

		if (string.IsNullOrWhiteSpace(Output))
			Output = Path.Combine(Path.GetTempPath(), $"{Team}-{Name}-{Version}.zip");

		if (File.Exists(Output))
			File.Delete(Output);

		ZipFile.CreateFromDirectory(tempDir, Output);

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
			var targetFile = file.ItemSpec;
			var outputFile = Path.Combine(destinationFolder, Path.GetFileName(targetFile));

			Directory.CreateDirectory(destinationFolder);
			File.Copy(targetFile, outputFile, true);
		}
	}

	private static string ResolveFilePath(ITaskItem file)
	{
		var destination = file.GetMetadata("Destination").Trim();

		if (string.IsNullOrEmpty(destination))
			return "";

		if (destination == ".")
			return "";

		if (destination.StartsWith("./"))
			destination = destination[2..];
		else if (destination.StartsWith('/'))
			destination = destination[1..];

		return destination;
	}

	[SuppressMessage(
		"Performance",
		"CA1859:Use concrete types when possible for improved performance"
	)]
	private static async Task<bool> ValidatePackage(
		IValidationService service,
		ILogger logger,
		Team team,
		string token,
		string sourceDir
	)
	{
		var errors = await service.ValidatePackage(
			team,
			Path.Combine(sourceDir, "icon.png"),
			Path.Combine(sourceDir, "manifest.json"),
			Path.Combine(sourceDir, "README.md"),
			token,
			CancellationToken.None
		);

		if (errors.Count == 0)
			return true;

		var output = new StringBuilder();

		output.AppendLine("Validation failed:");
		output.Append("- ");
		output.AppendJoin("\n- ", errors);

		logger.LogError("{Output}", output.ToString());
		return false;
	}
}
