using System.ComponentModel;
using System.Globalization;

namespace ThunderPipe.Infrastructure.TypeConverters;

/// <summary>
/// Converts a <see cref="string"/> to a valid absolute path
/// </summary>
internal sealed class PathTypeConverter : TypeConverter
{
	/// <inheritdoc />
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return sourceType == typeof(string) && base.CanConvertFrom(context, sourceType);
	}

	/// <inheritdoc />
	public override object? ConvertFrom(
		ITypeDescriptorContext? context,
		CultureInfo? culture,
		object value
	)
	{
		if (value is not string path)
			return base.ConvertFrom(context, culture, value);

		if (string.IsNullOrWhiteSpace(path))
			return path;

		path = Environment.ExpandEnvironmentVariables(path);

		if (path.StartsWith("~/"))
		{
			var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			path = Path.Combine(userProfilePath, path[2..]);
		}

		return Path.GetFullPath(path);
	}
}
