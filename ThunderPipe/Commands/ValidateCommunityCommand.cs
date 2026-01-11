using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Settings;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ValidateCommunityCommand : AsyncCommand<ValidateCommunitySettings>
{
	private readonly ILogger<PublishCommand> _logger;

	public ValidateCommunityCommand(ILogger<PublishCommand> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		ValidateCommunitySettings settings,
		CancellationToken cancellationToken
	)
	{
		var communitySlug = settings.Community;
		var builder = new RequestBuilder().ToUri(settings.Repository!);

		var community = await ThunderstoreAPI.FindCommunity(
			communitySlug,
			builder,
			cancellationToken
		);

		if (community == null)
		{
			_logger.LogError("Could not find a community with the slug '{Slug}'.", communitySlug);
			return 1;
		}

		_logger.LogInformation("'{Name}' community has been found!", community.Name);
		return 0;
	}
}
