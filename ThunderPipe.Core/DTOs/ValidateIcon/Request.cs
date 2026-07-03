using Newtonsoft.Json;

namespace ThunderPipe.Core.DTOs.ValidateIcon;

internal record Request
{
	/// <summary>
	/// String representation of the icon encoded with base-64 digits.
	/// </summary>
	[JsonProperty("icon_data")]
	[JsonRequired]
	public required string Data { get; init; }
}
