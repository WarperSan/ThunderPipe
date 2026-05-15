using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Commands.Fetch;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class LatestVersionCommand : AsyncCommand<Settings.Fetch.LatestVersionSettings>
{
	private readonly HttpApiClient _apiClient;
	private readonly ILogger _logger;

	public LatestVersionCommand(HttpApiClient apiClient, ILogger logger)
	{
		_apiClient = apiClient;
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		Settings.Fetch.LatestVersionSettings settings,
		CancellationToken cancellationToken
	)
	{
		var builder = new RequestBuilder().ToUri(settings.Host!);
		var client = new PackageApiClient(_apiClient, builder, _logger);

		var version = await client.GetVersion(settings.Team, settings.Name, cancellationToken);
		Console.WriteLine(version);

		return 0;
	}
}
