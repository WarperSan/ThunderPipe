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
	public void Convert_WhenHasImplicitCast_ReturnCastedValue()
	{
		const string EXPECTED = "contradiction ally squash thin";

		var converter = new StringCastTypeConverter<TypeWithImplicitCast>();

		Assert.True(converter.CanConvertFrom(typeof(string)));

		var convertedData = converter.ConvertFrom(EXPECTED) as TypeWithImplicitCast;
		Assert.NotNull(convertedData);
		Assert.Equal(EXPECTED, convertedData.ToString());
	}

	[Fact]
	public void Convert_WhenHasExplicitCast_ReturnCastedValue()
	{
		const string EXPECTED = "contradiction ally squash thin";

		var converter = new StringCastTypeConverter<TypeWithExplicitCast>();

		Assert.True(converter.CanConvertFrom(typeof(string)));

		var convertedData = converter.ConvertFrom(EXPECTED) as TypeWithExplicitCast;
		Assert.NotNull(convertedData);
		Assert.Equal(EXPECTED, convertedData.ToString());
	}

	[Fact]
	public void Convert_WhenHasNoCast_ReturnCantConvertFrom()
	{
		var converter = new StringCastTypeConverter<TypeWithNoCast>();

		Assert.False(converter.CanConvertFrom(typeof(string)));
	}

	[Fact]
	public void Convert_WhenHasNoCastButForced_ThrowException()
	{
		const string EXPECTED = "contradiction ally squash thin";

		var converter = new StringCastTypeConverter<TypeWithNoCast>();

		Assert.False(converter.CanConvertFrom(typeof(string)));

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
}
