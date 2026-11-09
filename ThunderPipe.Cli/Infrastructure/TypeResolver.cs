using Spectre.Console.Cli;

namespace ThunderPipe.Cli.Infrastructure;

internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
	/// <inheritdoc/>
	public object? Resolve(Type? type) => type == null ? null : provider.GetService(type);
}
