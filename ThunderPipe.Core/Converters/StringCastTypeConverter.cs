using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace ThunderPipe.Core.Converters;

/// <summary>
/// Converts <see cref="string"/> to and from <see cref="T"/> if a cast has been defined
/// </summary>
public sealed class StringCastTypeConverter<T> : TypeConverter
{
	private static readonly MethodInfo? ConvertFromMethod = GetConvertMethod(
		typeof(string),
		typeof(T)
	);
	private static readonly MethodInfo? ConvertToMethod = GetConvertMethod(
		typeof(T),
		typeof(string)
	);

	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		// ReSharper disable once ArrangeRedundantParentheses
		return (sourceType == typeof(string) && ConvertFromMethod != null)
			|| base.CanConvertFrom(context, sourceType);
	}

	/// <inheritdoc/>
	public override object? ConvertFrom(
		ITypeDescriptorContext? context,
		CultureInfo? culture,
		object value
	)
	{
		if (ConvertFromMethod == null)
			return base.ConvertFrom(context, culture, value);

		return ConvertFromMethod.Invoke(null, [value]);
	}

	/// <inheritdoc />
	public override bool CanConvertTo(
		ITypeDescriptorContext? context,
		[NotNullWhen(true)] Type? destinationType
	)
	{
		return destinationType == typeof(string) && ConvertToMethod != null;
	}

	/// <inheritdoc />
	public override object? ConvertTo(
		ITypeDescriptorContext? context,
		CultureInfo? culture,
		object? value,
		Type destinationType
	)
	{
		if (ConvertToMethod == null)
			throw new NotSupportedException($"Cannot convert '{typeof(T)}' to '{typeof(string)}'.");

		return ConvertToMethod.Invoke(null, [value]);
	}

	/// <summary>
	/// Gets the convert method if any was found
	/// </summary>
	private static MethodInfo? GetConvertMethod(Type source, Type target)
	{
		var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static);

		return methods.FirstOrDefault(IsConversion);

		bool IsConversion(MethodInfo method)
		{
			if (method.ReturnType != target)
				return false;

			if (method.GetParameters().FirstOrDefault()?.ParameterType != source)
				return false;

			return method.Name is "op_Implicit" or "op_Explicit";
		}
	}
}
