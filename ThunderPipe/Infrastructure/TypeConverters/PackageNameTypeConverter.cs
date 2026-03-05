using System.ComponentModel;
using System.Globalization;
using ThunderPipe.Models.Internal;

namespace ThunderPipe.Infrastructure.TypeConverters;

/// <summary>
/// Converts a <see cref="string"/> to a <see cref="PackageName"/>
/// </summary>
internal sealed class PackageNameTypeConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}

	/// <inheritdoc/>
	public override object? ConvertFrom(
		ITypeDescriptorContext? context,
		CultureInfo? culture,
		object value
	)
	{
		if (value is not string name)
			return base.ConvertFrom(context, culture, value);

		return new PackageName(name);
	}
}
