using Newtonsoft.Json;

namespace ThunderPipe.Core.Models.Web.ValidateIcon;

internal record Response
{
	/// <summary>
	/// Validity of the data
	/// </summary>
	[JsonProperty("success")]
	public required bool Valid { get; init; }
}
