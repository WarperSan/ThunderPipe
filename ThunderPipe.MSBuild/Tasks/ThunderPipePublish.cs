using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using ThunderPipe.Core.Models.API;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Utils;
using ThunderPipe.MSBuild.Tasks.Helpers;
using Task = Microsoft.Build.Utilities.Task;

namespace ThunderPipe.MSBuild.Tasks;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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

	public string[]? CommunityCategories { get; set; }

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

		if (string.IsNullOrEmpty(Host))
			Host = Core.Constants.DEFAULT_HOST;

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

		var communityCategories = ParseCommunitiesAndCategories(
			Communities.Select(c => (Community)c),
			CommunityCategories ?? [],
			Categories?.Select(x => (Category)x).ToArray() ?? []
		);
		var communities = communityCategories.Keys;

		var package = publicationService
			.PublishPackage(
				File,
				Team,
				communities,
				communityCategories,
				hasNsfw,
				Token,
				CancellationToken.None
			)
			.GetAwaiter()
			.GetResult();

		logger.LogInformation(
			"Successfully published '{VersionName}' v{VersionVersion}",
			package.Name,
			package.Version
		);

		Output = package.DownloadURL.ToString();
		return true;
	}

	private static Dictionary<Community, IEnumerable<Category>> ParseCommunitiesAndCategories(
		IEnumerable<Community> communities,
		string[] communityCategoriesStrings,
		IEnumerable<Category> sharedCategories
	)
	{
		const char SEPARATOR = '=';

		var communityCategories = new Dictionary<Community, List<Category>>();

		// Communities can be defined two ways: from Communities property, or
		// from CommunityCategories property, where the community is a key.
		foreach (var categoryString in communityCategoriesStrings)
		{
			var parts = categoryString.Split(SEPARATOR);

			if (parts.Length != 2)
				throw new IndexOutOfRangeException(
					$"Community category '{categoryString}' must have exactly one '{SEPARATOR}' separator."
				);

			var community = parts[0];
			var categoriesString = parts[1];

			// We can't use ';' as a separator here because that's the MSBuild array separator.
			var categories = categoriesString.Split(
				'/',
				StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
			);

			communityCategories[community] = [.. categories.Select(c => (Category)c)];
		}

		// Ensure all communities are included
		foreach (var community in communities)
		{
			if (!communityCategories.ContainsKey(community))
				communityCategories.Add(community, []);
		}

		// And finally add shared Categories to all communities
		foreach (var categories in communityCategories.Values)
		{
			categories.AddRange(sharedCategories);
		}

		return communityCategories.ToDictionary(x => x.Key, x => (IEnumerable<Category>)x.Value);
	}
}
