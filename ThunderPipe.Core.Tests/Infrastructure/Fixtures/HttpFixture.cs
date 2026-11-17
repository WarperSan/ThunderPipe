using System.Diagnostics.CodeAnalysis;
using ThunderPipe.Core.Tests.MockedObjects;
using ThunderPipe.Core.Utils;

namespace ThunderPipe.Core.Tests.Infrastructure.Fixtures;

/// <summary>
/// Fixture providing access to HTTP related object creators
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HttpFixture : IAsyncLifetime
{
	private const string URL = "http://localhost:5050";

	private readonly RequestBuilder _builder = new RequestBuilder().ToUri(new Uri(URL));

	/// <summary>
	/// Creates a new pre-configured instance of <see cref="TestMockHttpMessageHandler"/>
	/// </summary>
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	// ReSharper disable once MemberCanBeMadeStatic.Global
	public TestMockHttpMessageHandler MockHttp() => new(_builder);

	/// <summary>
	///	Creates a new pre-configured instance of <see cref="RequestBuilder"/>
	/// </summary>
	public RequestBuilder Builder() => new(_builder);

	/// <inheritdoc/>
	public ValueTask InitializeAsync() => ValueTask.CompletedTask;

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}
}
