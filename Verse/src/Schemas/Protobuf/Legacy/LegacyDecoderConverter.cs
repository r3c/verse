using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.DecoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf.Legacy
{
	/// <summary>
	/// Due to missing message type information when using "legacy" protobuf mode (only wire type is available) decoded
	/// values can only have 2 possible types: Signed & String. Converters will therefore trust caller for using the
	/// correct type and perform reinterpret casts instead of actual conversions.
	/// </summary>
	internal class LegacyDecoderConverter : IDecoderConverter<ProtobufValue>
	{
		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<ProtobufValue, bool>(LegacyDecoderConverter.ToBoolean) },
			{ typeof (char), new Converter<ProtobufValue, char>(LegacyDecoderConverter.ToCharacter) },
			{ typeof (decimal), new Converter<ProtobufValue, decimal>(LegacyDecoderConverter.ToDecimal) },
			{ typeof (float), new Converter<ProtobufValue, float>(LegacyDecoderConverter.ToFloat32) },
			{ typeof (double), new Converter<ProtobufValue, double>(LegacyDecoderConverter.ToFloat64) },
			{ typeof (sbyte), new Converter<ProtobufValue, sbyte>(LegacyDecoderConverter.ToInteger8s) },
			{ typeof (byte), new Converter<ProtobufValue, byte>(LegacyDecoderConverter.ToInteger8u) },
			{ typeof (short), new Converter<ProtobufValue, short>(LegacyDecoderConverter.ToInteger16s) },
			{ typeof (ushort), new Converter<ProtobufValue, ushort>(LegacyDecoderConverter.ToInteger16u) },
			{ typeof (int), new Converter<ProtobufValue, int>(LegacyDecoderConverter.ToInteger32s) },
			{ typeof (uint), new Converter<ProtobufValue, uint>(LegacyDecoderConverter.ToInteger32u) },
			{ typeof (long), new Converter<ProtobufValue, long>(LegacyDecoderConverter.ToInteger64s) },
			{ typeof (ulong), new Converter<ProtobufValue, ulong>(LegacyDecoderConverter.ToInteger64u) },
			{ typeof (string), new Converter<ProtobufValue, string>(LegacyDecoderConverter.ToString) },
			{ typeof (ProtobufValue), new Converter<ProtobufValue, ProtobufValue>(v => v) }
		};

		public Converter<ProtobufValue, TTo> Get<TTo>()
		{
			if (!this.converters.TryGetValue(typeof (TTo), out var box))
			{
				throw new InvalidCastException(
					string.Format(
						CultureInfo.InvariantCulture,
						"no available converter from Protobuf value to type '{0}'",
						typeof (TTo)));
			}

			return (Converter<ProtobufValue, TTo>)box;
		}

		public void Set<TTo>(Converter<ProtobufValue, TTo> converter)
		{
		    this.converters[typeof (TTo)] = converter ?? throw new ArgumentNullException(nameof(converter));
		}

		private static bool ToBoolean(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return value.Signed != 0;

				case ProtobufType.String:
					return !string.IsNullOrEmpty(value.String);

				default:
					return default;
			}
		}

		private static char ToCharacter(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (char) value.Signed;

				case ProtobufType.String:
					return value.String.Length > 0 ? value.String[0] : default;

				default:
					return default;
			}
		}

		private static unsafe decimal ToDecimal(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (decimal) *(double*) &value.Signed;

				case ProtobufType.String:
					return decimal.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static unsafe float ToFloat32(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					int v = (int) value.Signed;

					return *(float*)&v;

				case ProtobufType.String:
					return float.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static unsafe double ToFloat64(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return *(double*) &value.Signed;

				case ProtobufType.String:
					return double.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static sbyte ToInteger8s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (sbyte) value.Signed;

				case ProtobufType.String:
					return sbyte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static byte ToInteger8u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (byte) value.Signed;

				case ProtobufType.String:
					return byte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static short ToInteger16s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (short) value.Signed;

				case ProtobufType.String:
					return short.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static ushort ToInteger16u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (ushort) value.Signed;

				case ProtobufType.String:
					return ushort.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static int ToInteger32s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (int) value.Signed;

				case ProtobufType.String:
					return int.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static uint ToInteger32u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (uint) value.Signed;

				case ProtobufType.String:
					return uint.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static long ToInteger64s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return value.Signed;

				case ProtobufType.String:
					return long.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static ulong ToInteger64u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return (ulong) value.Signed;

				case ProtobufType.String:
					return ulong.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static string ToString(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Signed:
					return value.Signed.ToString(CultureInfo.InvariantCulture);

				case ProtobufType.String:
					return value.String;

				default:
					return string.Empty;
			}
		}
	}
}