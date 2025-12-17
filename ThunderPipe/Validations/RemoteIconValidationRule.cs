using System.Text;
using ThunderPipe.Utils;

namespace ThunderPipe.Validations;

/// <summary>
/// Rule to check if the icon respects remote validation rules
/// </summary>
internal sealed class RemoteIconValidationRule : BaseValidationRule
{
	private readonly string _iconPath;

	public RemoteIconValidationRule(string path)
	{
		_iconPath = path;
	}

	/// <inheritdoc />
	public override async Task<string?> Validate(
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var errors = await ThunderstoreAPI.ValidateIcon(_iconPath, builder, cancellationToken);

		if (errors == null)
			return "Failed to validate the icon remotely.";

		if (errors.DataErrors != null)
		{
			var dataErrorOutput = new StringBuilder();

			foreach (var error in errors.DataErrors)
				dataErrorOutput.AppendLine($"- {error}");

			return $"Icon transferring has resulted in errors:\n{dataErrorOutput}";
		}

		if (errors.ValidationErrors != null)
		{
			var dataErrorOutput = new StringBuilder();

			foreach (var error in errors.ValidationErrors)
				dataErrorOutput.AppendLine($"- {error}");

			return $"Icon has resulted in errors:\n{dataErrorOutput}";
		}

		if (errors.Valid is null or false)
			return "Icon is not marked as valid by the remote validator.";

		return null;
	}
}
