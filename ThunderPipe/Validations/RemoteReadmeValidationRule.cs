using System.Text;
using ThunderPipe.Utils;

namespace ThunderPipe.Validations;

/// <summary>
/// Rule to check if the icon respects remote validation rules
/// </summary>
internal sealed class RemoteReadmeValidationRule : BaseValidationRule
{
	private readonly string _readmePath;

	public RemoteReadmeValidationRule(string path)
	{
		_readmePath = path;
	}

	/// <inheritdoc />
	public override async Task<string?> Validate(
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var errors = await ThunderstoreAPI.ValidateReadme(_readmePath, builder, cancellationToken);

		if (errors == null)
			return "Failed to validate the README remotely.";

		if (errors.DataErrors != null)
		{
			var dataErrorOutput = new StringBuilder();

			foreach (var error in errors.DataErrors)
				dataErrorOutput.AppendLine($"- {error}");

			return $"README transferring has resulted in errors:\n{dataErrorOutput}";
		}

		if (errors.ValidationErrors != null)
		{
			var dataErrorOutput = new StringBuilder();

			foreach (var error in errors.ValidationErrors)
				dataErrorOutput.AppendLine($"- {error}");

			return $"README has resulted in errors:\n{dataErrorOutput}";
		}

		if (errors.Valid is null or false)
			return "README is not marked as valid by the remote validator.";

		return null;
	}
}
