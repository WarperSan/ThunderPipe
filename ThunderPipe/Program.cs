using Spectre.Console.Cli;
using ThunderPipe.Commands;

namespace ThunderPipe;

internal static class Program
{
	public static int Main(string[] args)
	{
		var app = new CommandApp();

		app.Configure(config =>
		{
			config.AddCommand<PublishCommand>("publish")
			      .WithDescription("Publish a package to Thunderstore.");
		});

		return app.Run(args);
	}
}