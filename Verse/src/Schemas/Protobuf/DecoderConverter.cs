using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.DecoderDescriptors.Base;

namespace Verse.Schemas.Protobuf
{
	class DecoderConverter : IDecoderConverter<ProtobufValue>
	{
		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<ProtobufValue, bool>(DecoderConverter.ToBoolean) },
			{ typeof (char), new Converter<ProtobufValue, char>(DecoderConverter.ToCharacter) },
			{ typeof (decimal), new Converter<ProtobufValue, decimal>(DecoderConverter.ToDecimal) },
			{ typeof (float), new Converter<ProtobufValue, float>(DecoderConverter.ToFloat32) },
			{ typeof (double), new Converter<ProtobufValue, double>(DecoderConverter.ToFloat64) },
			{ typeof (sbyte), new Converter<ProtobufValue, sbyte>(DecoderConverter.ToInteger8s) },
			{ typeof (byte), new Converter<ProtobufValue, byte>(DecoderConverter.ToInteger8u) },
			{ typeof (short), new Converter<ProtobufValue, short>(DecoderConverter.ToInteger16s) },
			{ typeof (ushort), new Converter<ProtobufValue, ushort>(DecoderConverter.ToInteger16u) },
			{ typeof (int), new Converter<ProtobufValue, int>(DecoderConverter.ToInteger32s) },
			{ typeof (uint), new Converter<ProtobufValue, uint>(DecoderConverter.ToInteger32u) },
			{ typeof (long), new Converter<ProtobufValue, long>(DecoderConverter.ToInteger64s) },
			{ typeof (ulong), new Converter<ProtobufValue, ulong>(DecoderConverter.ToInteger64u) },
			{ typeof (string), new Converter<ProtobufValue, string>(DecoderConverter.ToString) },
			{ typeof (ProtobufValue), new Converter<ProtobufValue, ProtobufValue>((v) => v) }
		};

		public Converter<ProtobufValue, TTo> Get<TTo>()
		{
			object box;

			if (!this.converters.TryGetValue(typeof (TTo), out box))
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
				case ProtobufType.Boolean:
					return value.Boolean;

				case ProtobufType.Float32:
					return Math.Abs(value.Float32) >= float.Epsilon;

				case ProtobufType.Float64:
					return Math.Abs(value.Float64) >= double.Epsilon;

				case ProtobufType.Signed:
					return value.Signed != 0;

				case ProtobufType.String:
					return !string.IsNullOrEmpty(value.String);

				case ProtobufType.Unsigned:
					return value.Unsigned != 0;

				default:
					return false;
			}
		}

		private static char ToCharacter(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? '1' : '\0';

				case ProtobufType.Float32:
					return (char)value.Float32;

				case ProtobufType.Float64:
					return (char)value.Float64;

				case ProtobufType.Signed:
					return (char)value.Signed;

				case ProtobufType.String:
					return value.String.Length > 0 ? value.String[0] : '\0';

				case ProtobufType.Unsigned:
					return (char)value.Unsigned;

				default:
					return '\0';
			}
		}

		private static decimal ToDecimal(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? 1 : 0;

				case ProtobufType.Float32:
					return (decimal)value.Float32;

				case ProtobufType.Float64:
					return (decimal)value.Float64;

				case ProtobufType.Signed:
					return value.Signed;

				case ProtobufType.String:
				    if (decimal.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return value.Unsigned;

				default:
					return 0;
			}
		}

		private static float ToFloat32(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? 1 : 0;

				case ProtobufType.Float32:
					return value.Float32;

				case ProtobufType.Float64:
					return (float)value.Float64;

				case ProtobufType.Signed:
					return value.Signed;

				case ProtobufType.String:
				    if (float.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return value.Unsigned;

				default:
					return 0;
			}
		}

		private static double ToFloat64(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? 1 : 0;

				case ProtobufType.Float32:
					return value.Float32;

				case ProtobufType.Float64:
					return value.Float64;

				case ProtobufType.Signed:
					return value.Signed;

				case ProtobufType.String:
				    if (double.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return value.Unsigned;

				default:
					return 0;
			}
		}

		private static sbyte ToInteger8s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? (sbyte)1 : (sbyte)0;

				case ProtobufType.Float32:
					return (sbyte)value.Float32;

				case ProtobufType.Float64:
					return (sbyte)value.Float64;

				case ProtobufType.Signed:
					return (sbyte)value.Signed;

				case ProtobufType.String:
				    if (sbyte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return (sbyte)value.Unsigned;

				default:
					return 0;
			}
		}

		private static byte ToInteger8u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? (byte)1 : (byte)0;

				case ProtobufType.Float32:
					return (byte)value.Float32;

				case ProtobufType.Float64:
					return (byte)value.Float64;

				case ProtobufType.Signed:
					return (byte)value.Signed;

				case ProtobufType.String:
				    if (byte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return (byte)value.Unsigned;

				default:
					return 0;
			}
		}

		private static short ToInteger16s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? (short)1 : (short)0;

				case ProtobufType.Float32:
					return (short)value.Float32;

				case ProtobufType.Float64:
					return (short)value.Float64;

				case ProtobufType.Signed:
					return (short)value.Signed;

				case ProtobufType.String:
				    if (short.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return (short)value.Unsigned;

				default:
					return 0;
			}
		}

		private static ushort ToInteger16u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? (ushort)1 : (ushort)0;

				case ProtobufType.Float32:
					return (ushort)value.Float32;

				case ProtobufType.Float64:
					return (ushort)value.Float64;

				case ProtobufType.Signed:
					return (ushort)value.Signed;

				case ProtobufType.String:
				    if (ushort.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return (ushort)value.Unsigned;

				default:
					return 0;
			}
		}

		private static int ToInteger32s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? 1 : 0;

				case ProtobufType.Float32:
					return (int)value.Float32;

				case ProtobufType.Float64:
					return (int)value.Float64;

				case ProtobufType.Signed:
					return (int)value.Signed;

				case ProtobufType.String:
				    if (int.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return (int)value.Unsigned;

				default:
					return 0;
			}
		}

		private static uint ToInteger32u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? 1u : 0u;

				case ProtobufType.Float32:
					return (uint)value.Float32;

				case ProtobufType.Float64:
					return (uint)value.Float64;

				case ProtobufType.Signed:
					return (uint)value.Signed;

				case ProtobufType.String:
				    if (uint.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return (uint)value.Unsigned;

				default:
					return 0;
			}
		}

		private static long ToInteger64s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? 1 : 0;

				case ProtobufType.Float32:
					return (long)value.Float32;

				case ProtobufType.Float64:
					return (long)value.Float64;

				case ProtobufType.Signed:
					return value.Signed;

				case ProtobufType.String:
				    if (long.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return (long)value.Unsigned;

				default:
					return 0;
			}
		}

		private static ulong ToInteger64u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? 1u : 0u;

				case ProtobufType.Float32:
					return (ulong)value.Float32;

				case ProtobufType.Float64:
					return (ulong)value.Float64;

				case ProtobufType.Signed:
					return (ulong)value.Signed;

				case ProtobufType.String:
				    if (ulong.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				case ProtobufType.Unsigned:
					return value.Unsigned;

				default:
					return 0;
			}
		}

		private static string ToString(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Boolean:
					return value.Boolean ? "1" : string.Empty;

				case ProtobufType.Float32:
					return value.Float32.ToString(CultureInfo.InvariantCulture);

				case ProtobufType.Float64:
					return value.Float64.ToString(CultureInfo.InvariantCulture);

				case ProtobufType.Signed:
					return value.Signed.ToString(CultureInfo.InvariantCulture);

				case ProtobufType.String:
					return value.String;

				case ProtobufType.Unsigned:
					return value.Unsigned.ToString(CultureInfo.InvariantCulture);

				default:
					return string.Empty;
			}
		}
	}
}