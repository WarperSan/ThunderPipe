using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using ThunderPipe.Commands;

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
			config.AddCommand<ValidateCommand>("validate").WithDescription("Validates a package");

			config
				.AddCommand<PublishCommand>("publish")
				.WithDescription("Publish a package to Thunderstore.");
		});

		return await app.RunAsync(args, cancellationTokenSource.Token);
	}
}
