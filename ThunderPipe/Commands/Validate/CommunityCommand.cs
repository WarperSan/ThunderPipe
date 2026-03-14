using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;
using ThunderPipe.Settings.Validate;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class CommunityCommand : BaseCommand<CommunitySettings>
{
	/// <inheritdoc />
	public CommunityCommand(ILogger logger)
		: base(logger) { }

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
		client.Logger = Logger;

		var doesCommunityExist = await client.Exists(communitySlug, cancellationToken);

		if (!doesCommunityExist)
			throw new KeyNotFoundException(
				$"Could not find a community with the slug '{communitySlug}'."
			);

		Logger.LogInformation("A community was found for '{Slug}'!", communitySlug);
		return 0;
	}
}
