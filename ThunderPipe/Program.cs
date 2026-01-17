using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace ThunderPipe;

internal static class Program
{
	public static async Task<int> Main(string[] args)
	{
		var cancellationTokenSource = new CancellationTokenSource();

		Console.CancelKeyPress += (_, e) =>
		{
			e.Cancel = true;
			cancellationTokenSource.Cancel();
			Console.WriteLine();
			Console.WriteLine("Cancellation requested...");
		};

		var services = new ServiceCollection();

		services.AddLogging(builder =>
		{
			builder.AddConsole();

#if DEBUG
			builder.SetMinimumLevel(LogLevel.Debug);
#else
			builder.SetMinimumLevel(LogLevel.Information);
#endif
		});

		var registrar = new Utils.TypeRegistrar(services);

		var app = new CommandApp(registrar);

		app.Configure(config =>
		{
			config.SetApplicationName(nameof(ThunderPipe));

			config.Settings.CaseSensitivity = CaseSensitivity.None;
			config.Settings.StrictParsing = false;

#if DEBUG
			config.PropagateExceptions();
			config.ValidateExamples();
#endif

			config.AddBranch("validate", ValidateBranch);
			config.AddBranch("create", CreateBranch);

			config
				.AddCommand<Commands.Publish.Command>("publish")
				.WithDescription("Publish a package to Thunderstore.");
		});

		return await app.RunAsync(args, cancellationTokenSource.Token);
	}

	private static void ValidateBranch(IConfigurator<Settings.Validate.BaseSettings> config)
	{
		config
			.AddCommand<Commands.Validate.PackageCommand>("package")
			.WithDescription("Checks if the package meets the server's requirements");

		config
			.AddCommand<Commands.Validate.CommunityCommand>("community")
			.WithDescription("Checks if a community slug exists");

		config
			.AddCommand<Commands.Validate.CategoriesCommand>("categories")
			.WithDescription("Checks if every category slug exists");

		config
			.AddCommand<Commands.Validate.DependenciesCommand>("dependencies")
			.WithDescription("Checks if every dependency exists");
	}

	private static void CreateBranch(IConfigurator<Settings.Create.BaseSettings> config)
	{
		config
			.AddCommand<Commands.Create.ManifestCommand>("manifest")
			.WithDescription("Creates a 'manifest.json' file");
	}
}
