using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.DecoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	/// <summary>
	/// Due to missing message type information when using "legacy" protobuf mode (only wire type is available) decoded
	/// values can only have 2 possible types: Signed & String. Converters will therefore trust caller for using the
	/// correct type and perform reinterpret casts instead of actual conversions.
	/// </summary>
	internal class RawProtobufDecoderConverter : IDecoderConverter<RawProtobufValue>
	{
		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<RawProtobufValue, bool>(RawProtobufDecoderConverter.ToBoolean) },
			{ typeof (char), new Converter<RawProtobufValue, char>(RawProtobufDecoderConverter.ToCharacter) },
			{ typeof (decimal), new Converter<RawProtobufValue, decimal>(RawProtobufDecoderConverter.ToDecimal) },
			{ typeof (float), new Converter<RawProtobufValue, float>(RawProtobufDecoderConverter.ToFloat32) },
			{ typeof (double), new Converter<RawProtobufValue, double>(RawProtobufDecoderConverter.ToFloat64) },
			{ typeof (sbyte), new Converter<RawProtobufValue, sbyte>(RawProtobufDecoderConverter.ToInteger8s) },
			{ typeof (byte), new Converter<RawProtobufValue, byte>(RawProtobufDecoderConverter.ToInteger8u) },
			{ typeof (short), new Converter<RawProtobufValue, short>(RawProtobufDecoderConverter.ToInteger16s) },
			{ typeof (ushort), new Converter<RawProtobufValue, ushort>(RawProtobufDecoderConverter.ToInteger16u) },
			{ typeof (int), new Converter<RawProtobufValue, int>(RawProtobufDecoderConverter.ToInteger32s) },
			{ typeof (uint), new Converter<RawProtobufValue, uint>(RawProtobufDecoderConverter.ToInteger32u) },
			{ typeof (long), new Converter<RawProtobufValue, long>(RawProtobufDecoderConverter.ToInteger64s) },
			{ typeof (ulong), new Converter<RawProtobufValue, ulong>(RawProtobufDecoderConverter.ToInteger64u) },
			{ typeof (string), new Converter<RawProtobufValue, string>(RawProtobufDecoderConverter.ToString) },
			{ typeof (RawProtobufValue), new Converter<RawProtobufValue, RawProtobufValue>(v => v) }
		};

		public Converter<RawProtobufValue, TTo> Get<TTo>()
		{
			if (!this.converters.TryGetValue(typeof (TTo), out var box))
			{
				throw new InvalidCastException(
					string.Format(
						CultureInfo.InvariantCulture,
						"no available converter from Protobuf value to type '{0}'",
						typeof (TTo)));
			}

			return (Converter<RawProtobufValue, TTo>)box;
		}

		public void Set<TTo>(Converter<RawProtobufValue, TTo> converter)
		{
		    this.converters[typeof (TTo)] = converter ?? throw new ArgumentNullException(nameof(converter));
		}

		private static bool ToBoolean(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return value.Number != 0;

				case RawProtobufWireType.String:
					return !string.IsNullOrEmpty(value.String);

				default:
					return default;
			}
		}

		private static char ToCharacter(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return (char) value.Number;

				case RawProtobufWireType.String:
					return value.String.Length > 0 ? value.String[0] : default;

				default:
					return default;
			}
		}

		private static unsafe decimal ToDecimal(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
					return (decimal) *(float*) &value.Number;

				case RawProtobufWireType.Fixed64:
					return (decimal) *(double*) &value.Number;

				case RawProtobufWireType.VarInt:
					return value.Number;

				case RawProtobufWireType.String:
					return decimal.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static unsafe float ToFloat32(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
					return *(float*) &value.Number;

				case RawProtobufWireType.Fixed64:
					return (float) *(double*) &value.Number;

				case RawProtobufWireType.VarInt:
					return value.Number;

				case RawProtobufWireType.String:
					return float.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static unsafe double ToFloat64(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
					return *(float*) &value.Number;

				case RawProtobufWireType.Fixed64:
					return *(double*) &value.Number;

				case RawProtobufWireType.VarInt:
					return value.Number;

				case RawProtobufWireType.String:
					return double.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static sbyte ToInteger8s(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return (sbyte) value.Number;

				case RawProtobufWireType.String:
					return sbyte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static byte ToInteger8u(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return (byte) value.Number;

				case RawProtobufWireType.String:
					return byte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static short ToInteger16s(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return (short) value.Number;

				case RawProtobufWireType.String:
					return short.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static ushort ToInteger16u(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return (ushort) value.Number;

				case RawProtobufWireType.String:
					return ushort.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static int ToInteger32s(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return (int) value.Number;

				case RawProtobufWireType.String:
					return int.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static uint ToInteger32u(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return (uint) value.Number;

				case RawProtobufWireType.String:
					return uint.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static long ToInteger64s(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return value.Number;

				case RawProtobufWireType.String:
					return long.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static unsafe ulong ToInteger64u(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
					return *(uint*) value.Number;

				case RawProtobufWireType.Fixed64:
					return *(ulong*) value.Number;

				case RawProtobufWireType.VarInt:
					return (ulong) value.Number;

				case RawProtobufWireType.String:
					return ulong.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture,
						out var number)
						? number
						: default;

				default:
					return default;
			}
		}

		private static string ToString(RawProtobufValue value)
		{
			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
				case RawProtobufWireType.Fixed64:
				case RawProtobufWireType.VarInt:
					return value.Number.ToString(CultureInfo.InvariantCulture);

				case RawProtobufWireType.String:
					return value.String;

				default:
					return string.Empty;
			}
		}
	}
}