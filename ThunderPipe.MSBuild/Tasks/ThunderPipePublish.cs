using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Utils;
using ThunderPipe.MSBuild.Tasks.Helpers;
using Task = Microsoft.Build.Utilities.Task;

namespace ThunderPipe.MSBuild.Tasks;

public class ThunderPipePublish : Task
{
	[Required]
	public required string Token { get; set; }

	[Required]
	public required string File { get; set; }

	[Required]
	public required string Team { get; set; }

	[Required]
	public required string[] Communities { get; set; }

	public string[]? Categories { get; set; }

	// ReSharper disable once InconsistentNaming
	public string? HasNSFW { get; set; }

	public string? Host { get; set; }

	[Output]
	public required string Output { get; set; }

	/// <inheritdoc />
	public override bool Execute()
	{
		var builder = new RequestBuilder();
		var logger = new MSBuildLogger(Log);

		if (!string.IsNullOrEmpty(Host))
			builder.ToUri(new Uri(Host));

		var publicationService = new PublicationService(builder, new FileSystem(), logger);

		if (!System.IO.File.Exists(File))
		{
			logger.LogError("Could not find the file at '{File}'.", File);
			return false;
		}

		var hasNsfw = false;

		if (HasNSFW != null && !bool.TryParse(HasNSFW, out hasNsfw))
		{
			logger.LogError("Could not parse '{HasNSFW}' as a boolean.", HasNSFW);
			return false;
		}

		var communities = Communities.Select(c => (Community)c).ToArray();
		var categories = ParseCategories(Categories ?? []);

		var package = publicationService
			.PublishPackage(
				File,
				Team,
				communities,
				categories,
				hasNsfw,
				Token,
				CancellationToken.None
			)
			.GetAwaiter()
			.GetResult();

		Output = package.DownloadURL.ToString();
		return true;
	}

	private static IDictionary<Community, IEnumerable<Category>> ParseCategories(
		string[] categoryStrings
	)
	{
		var categoriesDictionary = new Dictionary<Community, IEnumerable<Category>>();

		foreach (var categoryString in categoryStrings)
		{
			var parts = categoryString.Split('=');

			if (parts.Length < 2)
				continue;

			var community = parts[0];
			var categoriesString = parts[1];

			var categories = categoriesString.Split(
				';',
				StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
			);

			categoriesDictionary[community] = categories.Select(c => (Category)c);
		}

		return categoriesDictionary;
	}
}
