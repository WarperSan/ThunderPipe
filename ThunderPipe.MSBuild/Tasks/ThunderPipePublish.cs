using System.Runtime.InteropServices;
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

		var (communities, categories) = ParseCommunitiesAndCategories(
			Communities,
			CommunityCategories,
			Categories
		);

		// Debugging thing.
		// Probably would be good to have some simple way to confirm the values are correct as a user.
		// throw new Exception(
		// 	$"""
		// 		Token {Token}
		// 		File {File}
		// 		Team {Team}
		// 		Communities[] {string.Join(", ", communities.Select(x => x.ToString()))}
		// 		Categories[] {string.Join(
		// 		", ",
		// 		categoriesDictionary.Select(x => $"{x.Key}={string.Join(';', x.Value)}")
		// 	)}
		// 		HasNSFW {HasNSFW}
		// 		Host {Host}
		// 	"""
		// );

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

	private static (
		Community[] communities,
		Dictionary<Community, IEnumerable<Category>> categories
	) ParseCommunitiesAndCategories(
		string[] communities,
		string[]? communityCategories,
		string[]? categories
	)
	{
		// Communities can be defined two ways: from Communities property, or
		// from CommunityCategories property, where the community is a key.
		var communitiesHashSet = communities.Select(c => (Community)c).ToHashSet();
		var mutableCategoriesDictionary = ParseCommunityCategories(
			communityCategories ?? [],
			communitiesHashSet
		);
		var communitiesArray = communitiesHashSet.ToArray();

		// 'Categories' property is added to every community's categories,
		// even to those which were only defined as keys in CommunityCategories.
		AddSharedCategories(
			communitiesArray,
			mutableCategoriesDictionary,
			categories?.Select(x => (Category)x) ?? []
		);

		var categoriesDictionary = mutableCategoriesDictionary.ToDictionary(
			x => x.Key,
			x => (IEnumerable<Category>)x.Value
		);

		return (communitiesArray, categoriesDictionary);
	}

	private static Dictionary<Community, List<Category>> ParseCommunityCategories(
		string[] categoryStrings,
		HashSet<Community> communities
	)
	{
		const char SEPARATOR = '=';

		var categoriesDictionary = new Dictionary<Community, List<Category>>();

		foreach (var categoryString in categoryStrings)
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

			categoriesDictionary[community] = [.. categories.Select(c => (Category)c)];
			communities.Add(community);
		}

		return categoriesDictionary;
	}

	private static void AddSharedCategories(
		Community[] communities,
		Dictionary<Community, List<Category>> categoriesDictionary,
		IEnumerable<Category> sharedCategories
	)
	{
		foreach (var community in communities)
		{
			ref var categoryList = ref CollectionsMarshal.GetValueRefOrAddDefault(
				categoriesDictionary,
				community,
				out var exists
			);
			if (exists)
				categoryList!.AddRange(sharedCategories);
			else
				categoryList = [.. sharedCategories];
		}
	}
}
