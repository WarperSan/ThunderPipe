using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Settings.Validate;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class CommunityCommand : AsyncCommand<CommunitySettings>
{
	private readonly ILogger<CommunityCommand> _logger;

	public CommunityCommand(ILogger<CommunityCommand> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		CommunitySettings settings,
		CancellationToken cancellationToken
	)
	{
		var communitySlug = settings.Community;
		var builder = new RequestBuilder().ToUri(settings.Repository!);
		using var client = new CommunityApiClient(builder, new HttpClient(), cancellationToken);

		var doesCommunityExist = await client.Exists(communitySlug);

		if (!doesCommunityExist)
		{
			_logger.LogError("Could not find a community with the slug '{Slug}'.", communitySlug);
			return 1;
		}

		_logger.LogInformation("A community was found for '{Slug}'!", communitySlug);
		return 0;
	}
}
