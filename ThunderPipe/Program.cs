using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Spectre.Console.Cli;
using ThunderPipe.Core.Services.Implementations;
using ThunderPipe.Core.Services.Interfaces;
using ThunderPipe.Infrastructure;
using ThunderPipe.Infrastructure.Logging;

namespace ThunderPipe;

internal static class Program
{
	private const string TOKEN_EXAMPLE = "$THUNDERSTORE_TOKEN";

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
		var logLevelContext = new LogLevelContext();

		services.AddLogging(builder =>
		{
			builder.AddConsole(options =>
			{
				options.FormatterName = nameof(MinimalConsoleFormatter);
			});

			builder.AddConsoleFormatter<MinimalConsoleFormatter, ConsoleFormatterOptions>(options =>
			{
				options.IncludeScopes = false;
			});
			builder.AddFilter(level => level >= logLevelContext.Level);
		});
		services.AddSingleton<ILogger>(sp =>
			sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ThunderPipe))
		);

		services.AddSingleton<IFileSystem>(new FileSystem());

		var registrar = new TypeRegistrar(services);

		var app = new CommandApp(registrar);

		app.Configure(config =>
		{
			config.SetApplicationName(nameof(ThunderPipe));
			config.SetApplicationVersion(Metadata.VERSION);
			config.SetInterceptor(new LogInterceptor(logLevelContext));

			config.SetExceptionHandler(
				(exception, resolver) => HandleException(exception, resolver)
			);

			config.Settings.CaseSensitivity = CaseSensitivity.None;
			config.Settings.StrictParsing = false;

#if DEBUG
			config.PropagateExceptions();
			config.ValidateExamples();
#endif

			config.AddBranch("validate", ValidateBranch);
			config.AddBranch("create", CreateBranch);
			config.AddBranch("fetch", FetchBranch);

			config
				.AddCommand<Commands.Publish.PackageCommand>("publish")
				.WithDescription("Upload a zipped package to Thunderstore.")
				.WithExample(
					"publish",
					"my-mod.zip",
					"MyTeam",
					"risk-of-rain2",
					"--token",
					TOKEN_EXAMPLE
				)
				.WithExample(
					"publish",
					"my-mod.zip",
					"MyTeam",
					"lethal-company",
					"--token",
					TOKEN_EXAMPLE,
					"--category",
					"moons",
					"--has-nsfw"
				);
		});

		return await app.RunAsync(args, cancellationTokenSource.Token);
	}

	private static void ValidateBranch(IConfigurator<Settings.Validate.BaseValidateSettings> config)
	{
		config.SetDescription(
			"Check that values and package files meet Thunderstore's requirements"
		);

		config
			.AddCommand<Commands.Validate.PackageCommand>("package")
			.WithDescription("Check that a package and its metadata are valid before publishing")
			.WithExample("validate", "package", "./my-mod", "MyTeam", "--token", TOKEN_EXAMPLE);

		config
			.AddCommand<Commands.Validate.CommunityCommand>("community")
			.WithDescription(
				"Check that a community slug corresponds to an existing community on Thunderstore"
			)
			.WithExample("validate", "community", "risk-of-rain2")
			.WithExample("validate", "community", "lethal-company");

		config
			.AddCommand<Commands.Validate.CategoriesCommand>("categories")
			.WithDescription("Check that all category slugs exist within a given community")
			.WithExample("validate", "categories", "risk-of-rain2", "tools")
			.WithExample("validate", "categories", "risk-of-rain2", "tools", "mods");

		config
			.AddCommand<Commands.Validate.DependenciesCommand>("dependencies")
			.WithDescription(
				"Check that all dependency strings resolve to existing packages on Thunderstore"
			)
			.WithExample("validate", "dependencies", "AuthorName-PackageName-1.0.0")
			.WithExample(
				"validate",
				"dependencies",
				"AuthorName-PackageName-1.0.0",
				"OtherAuthor-OtherPackage-2.1.0"
			);
	}

	private static void CreateBranch(IConfigurator<Settings.Create.BaseCreateSettings> config)
	{
		config.SetDescription("Generate files required by Thunderstore packages");

		config
			.AddCommand<Commands.Create.ManifestCommand>("manifest")
			.WithDescription("Generate a manifest.json file with the required package metadata")
			.WithExample("create", "manifest", "MyMod", "1.0.0")
			.WithExample(
				"create",
				"manifest",
				"MyMod",
				"2.0.0",
				"--description",
				"\"A great mod\"",
				"--website",
				"https://thunderstore.io/"
			)
			.WithExample(
				"create",
				"manifest",
				"MyMod",
				"1.3.0",
				"--dependency",
				"AuthorName-PackageName-1.0.0",
				"--dependency",
				"OtherAuthor-OtherPackage-2.1.0"
			);
	}

	private static void FetchBranch(IConfigurator<Settings.Fetch.BaseFetchSettings> config)
	{
		config.SetDescription("Retrieve information about packages from Thunderstore");

		config
			.AddCommand<Commands.Fetch.LatestVersionCommand>("version")
			.WithDescription("Print the latest published version number of a package")
			.WithExample("fetch", "version", "MyTeam", "MyMod");
	}

	/// <summary>
	/// Handles incoming <see cref="Exception"/>
	/// </summary>
	/// <returns>Error code to return</returns>
	private static byte HandleException(Exception exception, ITypeResolver? resolver)
	{
		if (resolver?.Resolve(typeof(ILoggerFactory)) is ILoggerFactory factory)
		{
			var logger = factory.CreateLogger(exception.GetType());

			logger.LogError(exception, "{Message}", exception.Message);
		}

		return 1;
	}
}
