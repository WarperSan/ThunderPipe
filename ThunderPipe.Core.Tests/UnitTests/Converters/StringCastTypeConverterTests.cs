using ThunderPipe.Core.Converters;

namespace ThunderPipe.Tests.UnitTests.Converters;

public class StringCastTypeConverterTests
{
	public sealed record TypeWithImplicitCast
	{
		private readonly string _value;

		public TypeWithImplicitCast(string value)
		{
			_value = value;
		}

		/// <inheritdoc/>
		public override string ToString() => _value;

		public static implicit operator TypeWithImplicitCast(string value) => new(value);

		public static implicit operator string(TypeWithImplicitCast value) => value.ToString();
	}

	public sealed record TypeWithExplicitCast
	{
		private readonly string _value;

		public TypeWithExplicitCast(string value)
		{
			_value = value;
		}

		/// <inheritdoc/>
		public override string ToString() => _value;

		public static explicit operator TypeWithExplicitCast(string value) => new(value);

		public static explicit operator string(TypeWithExplicitCast value) => value.ToString();
	}

	// ReSharper disable once ClassNeverInstantiated.Global
	public sealed record TypeWithNoCast
	{
		private readonly string _value;

		public TypeWithNoCast(string value)
		{
			_value = value;
		}

		/// <inheritdoc/>
		public override string ToString() => _value;
	}

	[Fact]
	public void CanConvertFrom_WhenHasImplicitCast_ReturnTrue()
	{
		var converter = new StringCastTypeConverter<TypeWithImplicitCast>();

		Assert.True(converter.CanConvertFrom(typeof(string)));
	}

	[Fact]
	public void CanConvertFrom_WhenHasExplicitCast_ReturnTrue()
	{
		var converter = new StringCastTypeConverter<TypeWithExplicitCast>();

		Assert.True(converter.CanConvertFrom(typeof(string)));
	}

	[Fact]
	public void CanConvertFrom_WhenHasNoCast_ReturnFalse()
	{
		var converter = new StringCastTypeConverter<TypeWithNoCast>();

		Assert.False(converter.CanConvertFrom(typeof(string)));
	}

	[Fact]
	public void ConvertFrom_WhenHasImplicitCast_ReturnCastedValue()
	{
		const string EXPECTED = "contradiction ally squash thin";

		var converter = new StringCastTypeConverter<TypeWithImplicitCast>();

		var convertedData = converter.ConvertFrom(EXPECTED) as TypeWithImplicitCast;
		Assert.NotNull(convertedData);
		Assert.Equal(EXPECTED, convertedData.ToString());
	}

	[Fact]
	public void ConvertFrom_WhenHasExplicitCast_ReturnCastedValue()
	{
		const string EXPECTED = "contradiction ally squash thin";

		var converter = new StringCastTypeConverter<TypeWithExplicitCast>();

		var convertedData = converter.ConvertFrom(EXPECTED) as TypeWithExplicitCast;
		Assert.NotNull(convertedData);
		Assert.Equal(EXPECTED, convertedData.ToString());
	}

	[Fact]
	public void ConvertFrom_WhenHasNoCast_ThrowException()
	{
		const string EXPECTED = "contradiction ally squash thin";

		var converter = new StringCastTypeConverter<TypeWithNoCast>();

		try
		{
			_ = converter.ConvertFrom(EXPECTED);
			Assert.Fail("Should have thrown");
		}
		catch (Exception e)
		{
			Assert.IsType<NotSupportedException>(e);
		}
	}

	[Fact]
	public void CanConvertTo_WhenHasImplicitCast_ReturnTrue()
	{
		var converter = new StringCastTypeConverter<TypeWithImplicitCast>();

		Assert.True(converter.CanConvertTo(typeof(string)));
	}

	[Fact]
	public void CanConvertTo_WhenHasExplicitCast_ReturnTrue()
	{
		var converter = new StringCastTypeConverter<TypeWithExplicitCast>();

		Assert.True(converter.CanConvertTo(typeof(string)));
	}

	[Fact]
	public void CanConvertTo_WhenHasNoCast_ReturnFalse()
	{
		var converter = new StringCastTypeConverter<TypeWithNoCast>();

		Assert.False(converter.CanConvertTo(typeof(string)));
	}

	[Fact]
	public void ConvertTo_WhenHasImplicitCast_ReturnCastedValue()
	{
		const string EXPECTED = "contradiction ally squash thin";

		object data = new TypeWithImplicitCast(EXPECTED);

		var converter = new StringCastTypeConverter<TypeWithImplicitCast>();

		var convertedData = converter.ConvertTo(data, typeof(string)) as string;
		Assert.NotNull(convertedData);
		Assert.Equal(EXPECTED, convertedData);
	}

	[Fact]
	public void ConvertTo_WhenHasExplicitCast_ReturnCastedValue()
	{
		const string EXPECTED = "contradiction ally squash thin";

		object data = new TypeWithExplicitCast(EXPECTED);

		var converter = new StringCastTypeConverter<TypeWithExplicitCast>();

		var convertedData = converter.ConvertTo(data, typeof(string)) as string;
		Assert.NotNull(convertedData);
		Assert.Equal(EXPECTED, convertedData);
	}

	[Fact]
	public void ConvertTo_WhenHasNoCast_ThrowException()
	{
		const string EXPECTED = "contradiction ally squash thin";

		object data = new TypeWithNoCast(EXPECTED);

		var converter = new StringCastTypeConverter<TypeWithNoCast>();

		try
		{
			var c = converter.ConvertTo(data, typeof(string)) as string;
			Assert.Fail("Should have thrown");
		}
		catch (Exception e)
		{
			Assert.IsType<NotSupportedException>(e);
		}
	}
}
