using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Clients;
using ThunderPipe.Settings.Validate;
using ThunderPipe.Utils;

namespace ThunderPipe.Commands.Validate;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class CategoriesCommand : AsyncCommand<CategoriesSettings>
{
	private readonly ILogger<CategoriesCommand> _logger;

	public CategoriesCommand(ILogger<CategoriesCommand> logger)
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
		var builder = new RequestBuilder().ToUri(settings.Repository!);
		using var client = new CategoryApiClient(builder, cancellationToken);

		var missingCategories = await client.GetMissing(settings.Categories!, settings.Community);

		if (missingCategories.Count > 0)
		{
			var listString = "- " + string.Join("\n- ", missingCategories);

			_logger.LogError("Failed to find these categories:\n{Categories}", listString);
			return 1;
		}

		_logger.LogInformation("All categories have been found!");
		return 0;
	}
}
