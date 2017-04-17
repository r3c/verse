using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.Schemas.Protobuf
{
	class DecoderConverter : IDecoderConverter<ProtobufValue>
	{
		#region Attributes

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

		#endregion

		#region Methods / Public

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
			if (converter == null)
				throw new ArgumentNullException("converter");

			this.converters[typeof (TTo)] = converter;
		}

		#endregion

		#region Methods / Private

		private static bool ToBoolean(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return Math.Abs(value.FloatContent) >= float.Epsilon;

				case ProtobufType.Double:
					return Math.Abs(value.DoubleContent) >= double.Epsilon;

				case ProtobufType.Long:
					return value.LongContent != 0;

				case ProtobufType.String:
					return !string.IsNullOrEmpty(value.StringContent);

				default:
					return false;
			}
		}

		private static char ToCharacter(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return Math.Abs(value.FloatContent) >= float.Epsilon ? '1' : '\0';

				case ProtobufType.Double:
					return Math.Abs(value.DoubleContent) >= double.Epsilon ? '1' : '\0';

				case ProtobufType.Long:
					return value.LongContent != 0 ? '1' : '\0';

				case ProtobufType.String:
					return value.StringContent.Length > 0 ? value.StringContent[0] : '\0';

				default:
					return '\0';
			}
		}

		private static decimal ToDecimal(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (decimal)value.FloatContent;

				case ProtobufType.Double:
					return (decimal)value.DoubleContent;

				case ProtobufType.Long:
					return value.LongContent;

				case ProtobufType.String:
					decimal number;

					if (decimal.TryParse(value.StringContent, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static float ToFloat32(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return value.FloatContent;

				case ProtobufType.Double:
					return (float)value.DoubleContent;

				case ProtobufType.Long:
					return value.LongContent;

				case ProtobufType.String:
					float number;

					if (float.TryParse(value.StringContent, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static double ToFloat64(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return value.FloatContent;

				case ProtobufType.Double:
					return value.DoubleContent;

				case ProtobufType.Long:
					return value.LongContent;

				case ProtobufType.String:
					double number;

					if (double.TryParse(value.StringContent, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static sbyte ToInteger8s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (sbyte)value.FloatContent;

				case ProtobufType.Double:
					return (sbyte)value.DoubleContent;

				case ProtobufType.Long:
					return (sbyte)value.LongContent;

				case ProtobufType.String:
					sbyte number;

					if (sbyte.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static byte ToInteger8u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (byte)value.FloatContent;

				case ProtobufType.Double:
					return (byte)value.DoubleContent;

				case ProtobufType.Long:
					return (byte)value.LongContent;

				case ProtobufType.String:
					byte number;

					if (byte.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static short ToInteger16s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (short)value.FloatContent;

				case ProtobufType.Double:
					return (short)value.DoubleContent;

				case ProtobufType.Long:
					return (short)value.LongContent;

				case ProtobufType.String:
					short number;

					if (short.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ushort ToInteger16u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (ushort)value.FloatContent;

				case ProtobufType.Double:
					return (ushort)value.DoubleContent;

				case ProtobufType.Long:
					return (ushort)value.LongContent;

				case ProtobufType.String:
					ushort number;

					if (ushort.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static int ToInteger32s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (int)value.FloatContent;

				case ProtobufType.Double:
					return (int)value.DoubleContent;

				case ProtobufType.Long:
					return (int)value.LongContent;

				case ProtobufType.String:
					int number;

					if (int.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static uint ToInteger32u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (uint)value.FloatContent;

				case ProtobufType.Double:
					return (uint)value.DoubleContent;

				case ProtobufType.Long:
					return (uint)value.LongContent;

				case ProtobufType.String:
					uint number;

					if (uint.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static long ToInteger64s(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (long)value.FloatContent;

				case ProtobufType.Double:
					return (long)value.DoubleContent;

				case ProtobufType.Long:
					return value.LongContent;

				case ProtobufType.String:
					long number;

					if (long.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ulong ToInteger64u(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return (ulong)value.FloatContent;

				case ProtobufType.Double:
					return (ulong)value.DoubleContent;

				case ProtobufType.Long:
					return (ulong)value.LongContent;

				case ProtobufType.String:
					ulong number;

					if (ulong.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static string ToString(ProtobufValue value)
		{
			switch (value.Type)
			{
				case ProtobufType.Float:
					return value.FloatContent.ToString(CultureInfo.InvariantCulture);

				case ProtobufType.Double:
					return value.DoubleContent.ToString(CultureInfo.InvariantCulture);

				case ProtobufType.Long:
					return value.LongContent.ToString(CultureInfo.InvariantCulture);

				case ProtobufType.String:
					return value.StringContent;

				default:
					return string.Empty;
			}
		}

		#endregion
	}
}