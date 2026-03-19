using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;
using ThunderPipe.Settings.Validate;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class CommunityCommand : AsyncCommand<CommunitySettings>
{
	private readonly ILogger _logger;

	public CommunityCommand(ILogger logger)
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
		var community = settings.Community;
		var builder = new RequestBuilder().ToUri(settings.Host!);
		using var client = new CommunityApiClient();
		client.Builder = builder;
		client.Logger = _logger;

		var doesCommunityExist = await client.Exists(community, cancellationToken);

		if (!doesCommunityExist)
			throw new KeyNotFoundException(
				$"Could not find a community with the slug '{community}'."
			);

		_logger.LogInformation("A community was found for '{Slug}'!", community);
		return 0;
	}
}
