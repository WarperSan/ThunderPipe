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
		var builder = new RequestBuilder().ToUri(settings.Host!);
		using var client = new CommunityApiClient();
		client.Builder = builder;
		client.CancellationToken = cancellationToken;

		var doesCommunityExist = await client.Exists(communitySlug);

		if (!doesCommunityExist)
			throw new KeyNotFoundException(
				$"Could not find a community with the slug '{communitySlug}'."
			);

		_logger.LogInformation("A community was found for '{Slug}'!", communitySlug);
		return 0;
	}
}
