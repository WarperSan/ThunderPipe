using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace ThunderPipe.Core.Converters;

/// <summary>
/// Converts <see cref="string"/> into <see cref="T"/> if a cast has been defined
/// </summary>
public sealed class StringCastTypeConverter<T> : TypeConverter
{
	private readonly MethodInfo? _convertMethod = GetConvertMethod();

	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		if (sourceType != typeof(string))
			return false;

		return _convertMethod != null;
	}

	/// <inheritdoc/>
	public override object? ConvertFrom(
		ITypeDescriptorContext? context,
		CultureInfo? culture,
		object value
	)
	{
		if (_convertMethod == null)
			return base.ConvertFrom(context, culture, value);

		return _convertMethod.Invoke(null, [value]);
	}

	/// <summary>
	/// Gets the convert method if any was found
	/// </summary>
	private static MethodInfo? GetConvertMethod()
	{
		var target = typeof(T);
		var methods = target.GetMethods(BindingFlags.Public | BindingFlags.Static);

		return methods.FirstOrDefault(IsConversion);

		bool IsConversion(MethodInfo method)
		{
			if (method.ReturnType != target)
				return false;

			if (method.GetParameters().FirstOrDefault()?.ParameterType != typeof(string))
				return false;

			return method.Name is "op_Implicit" or "op_Explicit";
		}
	}
}
