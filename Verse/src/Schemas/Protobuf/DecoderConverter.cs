using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.Schemas.Protobuf
{
	class DecoderConverter : IDecoderConverter<Value>
	{
		#region Attributes

		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<Value, bool>(DecoderConverter.ToBoolean) },
			{ typeof (char), new Converter<Value, char>(DecoderConverter.ToCharacter) },
			{ typeof (decimal), new Converter<Value, decimal>(DecoderConverter.ToDecimal) },
			{ typeof (float), new Converter<Value, float>(DecoderConverter.ToFloat32) },
			{ typeof (double), new Converter<Value, double>(DecoderConverter.ToFloat64) },
			{ typeof (sbyte), new Converter<Value, sbyte>(DecoderConverter.ToInteger8s) },
			{ typeof (byte), new Converter<Value, byte>(DecoderConverter.ToInteger8u) },
			{ typeof (short), new Converter<Value, short>(DecoderConverter.ToInteger16s) },
			{ typeof (ushort), new Converter<Value, ushort>(DecoderConverter.ToInteger16u) },
			{ typeof (int), new Converter<Value, int>(DecoderConverter.ToInteger32s) },
			{ typeof (uint), new Converter<Value, uint>(DecoderConverter.ToInteger32u) },
			{ typeof (long), new Converter<Value, long>(DecoderConverter.ToInteger64s) },
			{ typeof (ulong), new Converter<Value, ulong>(DecoderConverter.ToInteger64u) },
			{ typeof (string), new Converter<Value, string>(DecoderConverter.ToString) },
			{ typeof (Value), new Converter<Value, Value>((v) => v) }
		};

		#endregion

		#region Methods / Public

		public Converter<Value, TTo> Get<TTo>()
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

			return (Converter<Value, TTo>)box;
		}

		public void Set<TTo>(Converter<Value, TTo> converter)
		{
			if (converter == null)
				throw new ArgumentNullException("converter");

			this.converters[typeof (TTo)] = converter;
		}

		#endregion

		#region Methods / Private

		private static bool ToBoolean(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return Math.Abs(value.FloatContent) >= float.Epsilon;

				case ContentType.Double:
					return Math.Abs(value.DoubleContent) >= double.Epsilon;

				case ContentType.Long:
					return value.LongContent != 0;

				case ContentType.String:
					return !string.IsNullOrEmpty(value.StringContent);

				default:
					return false;
			}
		}

		private static char ToCharacter(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return Math.Abs(value.FloatContent) >= float.Epsilon ? '1' : '\0';

				case ContentType.Double:
					return Math.Abs(value.DoubleContent) >= double.Epsilon ? '1' : '\0';

				case ContentType.Long:
					return value.LongContent != 0 ? '1' : '\0';

				case ContentType.String:
					return value.StringContent.Length > 0 ? value.StringContent[0] : '\0';

				default:
					return '\0';
			}
		}

		private static decimal ToDecimal(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (decimal)value.FloatContent;

				case ContentType.Double:
					return (decimal)value.DoubleContent;

				case ContentType.Long:
					return value.LongContent;

				case ContentType.String:
					decimal number;

					if (decimal.TryParse(value.StringContent, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static float ToFloat32(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return value.FloatContent;

				case ContentType.Double:
					return (float)value.DoubleContent;

				case ContentType.Long:
					return value.LongContent;

				case ContentType.String:
					float number;

					if (float.TryParse(value.StringContent, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static double ToFloat64(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return value.FloatContent;

				case ContentType.Double:
					return value.DoubleContent;

				case ContentType.Long:
					return value.LongContent;

				case ContentType.String:
					double number;

					if (double.TryParse(value.StringContent, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static sbyte ToInteger8s(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (sbyte)value.FloatContent;

				case ContentType.Double:
					return (sbyte)value.DoubleContent;

				case ContentType.Long:
					return (sbyte)value.LongContent;

				case ContentType.String:
					sbyte number;

					if (sbyte.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static byte ToInteger8u(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (byte)value.FloatContent;

				case ContentType.Double:
					return (byte)value.DoubleContent;

				case ContentType.Long:
					return (byte)value.LongContent;

				case ContentType.String:
					byte number;

					if (byte.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static short ToInteger16s(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (short)value.FloatContent;

				case ContentType.Double:
					return (short)value.DoubleContent;

				case ContentType.Long:
					return (short)value.LongContent;

				case ContentType.String:
					short number;

					if (short.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ushort ToInteger16u(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (ushort)value.FloatContent;

				case ContentType.Double:
					return (ushort)value.DoubleContent;

				case ContentType.Long:
					return (ushort)value.LongContent;

				case ContentType.String:
					ushort number;

					if (ushort.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static int ToInteger32s(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (int)value.FloatContent;

				case ContentType.Double:
					return (int)value.DoubleContent;

				case ContentType.Long:
					return (int)value.LongContent;

				case ContentType.String:
					int number;

					if (int.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static uint ToInteger32u(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (uint)value.FloatContent;

				case ContentType.Double:
					return (uint)value.DoubleContent;

				case ContentType.Long:
					return (uint)value.LongContent;

				case ContentType.String:
					uint number;

					if (uint.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static long ToInteger64s(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (long)value.FloatContent;

				case ContentType.Double:
					return (long)value.DoubleContent;

				case ContentType.Long:
					return value.LongContent;

				case ContentType.String:
					long number;

					if (long.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ulong ToInteger64u(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return (ulong)value.FloatContent;

				case ContentType.Double:
					return (ulong)value.DoubleContent;

				case ContentType.Long:
					return (ulong)value.LongContent;

				case ContentType.String:
					ulong number;

					if (ulong.TryParse(value.StringContent, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static string ToString(Value value)
		{
			switch (value.Type)
			{
				case ContentType.Float:
					return value.FloatContent.ToString(CultureInfo.InvariantCulture);

				case ContentType.Double:
					return value.DoubleContent.ToString(CultureInfo.InvariantCulture);

				case ContentType.Long:
					return value.LongContent.ToString(CultureInfo.InvariantCulture);

				case ContentType.String:
					return value.StringContent;

				default:
					return string.Empty;
			}
		}

		#endregion
	}
}