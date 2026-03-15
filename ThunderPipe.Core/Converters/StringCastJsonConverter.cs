using Newtonsoft.Json;

namespace ThunderPipe.Core.Converters;

/// <summary>
/// Converts <see cref="string"/> to and from <see cref="T"/> if a cast has been defined
/// </summary>
internal sealed class StringCastJsonConverter<T> : JsonConverter
{
	private readonly StringCastTypeConverter<T> _converter = new();

	/// <inheritdoc />
	public override bool CanRead => false;

	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		var valueString = _converter.ConvertTo(value, typeof(string));
		writer.WriteValue(valueString);
	}

	/// <inheritdoc />
	public override object ReadJson(
		JsonReader reader,
		Type objectType,
		object? existingValue,
		JsonSerializer serializer
	) => throw new InvalidOperationException();

	/// <inheritdoc />
	public override bool CanConvert(Type objectType) => _converter.CanConvertTo(objectType);
}
