using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Core.Clients;
using ThunderPipe.Core.Utils;
using ThunderPipe.Settings.Validate;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class CategoriesCommand : AsyncCommand<CategoriesSettings>
{
	private readonly ILogger _logger;

	public CategoriesCommand(ILogger logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override async Task<int> ExecuteAsync(
		CommandContext context,
		CategoriesSettings settings,
		CancellationToken cancellationToken
	)
	{
		var builder = new RequestBuilder().ToUri(settings.Host!);
		using var client = new CategoryApiClient();
		client.Builder = builder;
		client.Logger = _logger;

		var missingCategories = await client.GetMissing(
			settings.Categories,
			settings.Community,
			cancellationToken
		);

		if (missingCategories.Count > 0)
		{
			var listString = "- " + string.Join("\n- ", missingCategories);

			throw new KeyNotFoundException(
				$"Could not find a category for the following slugs:\n{listString}"
			);
		}

		_logger.LogInformation("All categories have been found!");
		return 0;
	}
}
