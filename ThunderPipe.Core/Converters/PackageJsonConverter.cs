using Newtonsoft.Json;
using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Core.Converters;

/// <summary>
/// Converts package types to and from JSON
/// </summary>
internal sealed class PackageJsonConverter : JsonConverter
{
	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		switch (value)
		{
			case PackageName packageName:
				writer.WriteValue(packageName);
				break;
			case PackageVersion packageVersion:
				writer.WriteValue(packageVersion);
				break;
			case PackageDependency packageDependency:
				writer.WriteValue(packageDependency);
				break;
			default:
				throw new NotSupportedException();
		}
	}

	/// <inheritdoc />
	public override object? ReadJson(
		JsonReader reader,
		Type objectType,
		object? existingValue,
		JsonSerializer serializer
	) => throw new NotImplementedException();

	/// <inheritdoc />
	public override bool CanConvert(Type objectType)
	{
		if (objectType == typeof(PackageName))
			return true;

		if (objectType == typeof(PackageVersion))
			return true;

		if (objectType == typeof(PackageDependency))
			return true;

		return false;
	}
}
