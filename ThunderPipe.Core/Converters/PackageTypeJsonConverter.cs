using Newtonsoft.Json;
using ThunderPipe.Core.Models.API;

namespace ThunderPipe.Core.Converters;

/// <summary>
/// Converts package types to and from JSON
/// </summary>
internal sealed class PackageTypeJsonConverter : JsonConverter
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
			case Community community:
				writer.WriteValue(community);
				break;
			case Category category:
				writer.WriteValue(category);
				break;
			case Team team:
				writer.WriteValue(team);
				break;
			default:
				throw new NotSupportedException();
		}
	}

	/// <inheritdoc />
	public override object ReadJson(
		JsonReader reader,
		Type objectType,
		object? existingValue,
		JsonSerializer serializer
	) => throw new NotImplementedException();

	/// <inheritdoc />
	public override bool CanConvert(Type objectType) =>
		objectType == typeof(PackageName)
		|| objectType == typeof(PackageVersion)
		|| objectType == typeof(PackageDependency)
		|| objectType == typeof(Community)
		|| objectType == typeof(Team)
		|| objectType == typeof(Category);
}
