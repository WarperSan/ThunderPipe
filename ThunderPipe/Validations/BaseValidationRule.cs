using ThunderPipe.Utils;

namespace ThunderPipe.Validations;

/// <summary>
/// Class that checks if a certain rule is valid or not
/// </summary>
internal abstract class BaseValidationRule
{
	/// <summary>
	/// Checks if this rule is valid
	/// </summary>
	public abstract Task<string?> Validate(RequestBuilder builder, CancellationToken cancellationToken);
}
