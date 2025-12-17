using System.Text;
using ThunderPipe.Utils;

namespace ThunderPipe.Validations;

/// <summary>
/// Rule to check if the manifest respects remote validation rules
/// </summary>
internal sealed class RemoteManifestValidationRule : BaseValidationRule
{
	private readonly string _manifestPath;
	private readonly string _author;

	public RemoteManifestValidationRule(string path, string author)
	{
		_manifestPath = path;
		_author = author;
	}

	/// <inheritdoc />
	public override async Task<string?> Validate(
		RequestBuilder builder,
		CancellationToken cancellationToken
	)
	{
		var errors = await ThunderstoreAPI.ValidateManifest(
			_manifestPath,
			_author,
			builder,
			cancellationToken
		);

		if (errors == null)
			return "Failed to validate the manifest remotely.";

		if (errors.DataErrors != null)
		{
			var dataErrorOutput = new StringBuilder();

			foreach (var error in errors.DataErrors)
				dataErrorOutput.AppendLine($"- {error}");

			return $"Manifest transferring has resulted in errors:\n{dataErrorOutput}";
		}

		if (errors.NamespaceErrors != null)
		{
			var dataErrorOutput = new StringBuilder();

			foreach (var error in errors.NamespaceErrors)
				dataErrorOutput.AppendLine($"- {error}");

			return $"Manifest 'namespace' has resulted in errors:\n{dataErrorOutput}";
		}

		if (errors.ValidationErrors != null)
		{
			var dataErrorOutput = new StringBuilder();

			foreach (var error in errors.ValidationErrors)
				dataErrorOutput.AppendLine($"- {error}");

			return $"Manifest has resulted in errors:\n{dataErrorOutput}";
		}

		if (errors.Valid is null or false)
			return "Manifest is not marked as valid by the remote validator.";

		return null;
	}
}
