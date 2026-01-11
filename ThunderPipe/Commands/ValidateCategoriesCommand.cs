using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ValidateCategoriesCommand : AsyncCommand<ValidateCategoriesSettings>
{
	private readonly ILogger<PublishCommand> _logger;

	public ValidateCategoriesCommand(ILogger<PublishCommand> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		ValidateCategoriesSettings settings,
		CancellationToken cancellationToken
	)
	{
		var categorySlugs = settings.Categories!;
		var communitySlug = settings.Community;
		var builder = new RequestBuilder().ToUri(settings.Repository!);

		var categories = await ThunderstoreAPI.FindCategories(
			categorySlugs,
			communitySlug,
			builder,
			cancellationToken
		);

		var missingCategories = categorySlugs.Where(c => !categories.ContainsKey(c));

		if (missingCategories.Any())
		{
			var listString = "- " + string.Join("\n- ", missingCategories);

			_logger.LogError("Failed to find these categories:\n{Categories}", listString);
			return 1;
		}

		_logger.LogInformation("All categories have been found!");
		return 0;
	}
}
