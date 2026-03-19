using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;

namespace ThunderPipe.Core.Converters;

// TODO: Make it so TypeDescriptor sees this type in MSBuild.Tasks

/// <summary>
/// Converts <see cref="string"/> to and from <see cref="T"/> if a cast has been defined
/// </summary>
public sealed class StringCastTypeConverter<T> : TypeConverter
{
	private static readonly Func<string, T>? ConvertFromMethod = GetConvertMethod<string, T>();
	private static readonly Func<T, string>? ConvertToMethod = GetConvertMethod<T, string>();

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
		if (value is not string valueString || ConvertFromMethod == null)
			return base.ConvertFrom(context, culture, value);

		return ConvertFromMethod.Invoke(valueString);
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
		if (value is not T valueTyped || ConvertToMethod == null)
			return base.ConvertTo(context, culture, value, destinationType);

		return ConvertToMethod.Invoke(valueTyped);
	}

	/// <summary>
	/// Gets the convert method if any was found
	/// </summary>
	private static Func<TSource, TTarget>? GetConvertMethod<TSource, TTarget>()
	{
		try
		{
			var parameter = Expression.Parameter(typeof(TSource));
			var body = Expression.Convert(parameter, typeof(TTarget));

			return Expression.Lambda<Func<TSource, TTarget>>(body, parameter).Compile();
		}
		catch (Exception)
		{
			return null;
		}
	}
}
