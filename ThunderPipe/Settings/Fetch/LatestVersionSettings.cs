using System.Diagnostics.CodeAnalysis;
using ThunderPipe.Commands.Fetch;

namespace ThunderPipe.Settings.Fetch;

/// <summary>
/// Settings used by <see cref="LatestVersionCommand"/>
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class LatestVersionSettings : BaseSettings;
