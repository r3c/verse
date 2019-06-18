using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.DecoderDescriptors.Tree;

namespace Verse.Schemas.JSON
{
	internal class DecoderConverter : IDecoderConverter<JSONValue>
	{
		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<JSONValue, bool>(DecoderConverter.ToBoolean) },
			{ typeof (char), new Converter<JSONValue, char>(DecoderConverter.ToCharacter) },
			{ typeof (decimal), new Converter<JSONValue, decimal>(DecoderConverter.ToDecimal) },
			{ typeof (float), new Converter<JSONValue, float>(DecoderConverter.ToFloat32) },
			{ typeof (double), new Converter<JSONValue, double>(DecoderConverter.ToFloat64) },
			{ typeof (sbyte), new Converter<JSONValue, sbyte>(DecoderConverter.ToInteger8s) },
			{ typeof (byte), new Converter<JSONValue, byte>(DecoderConverter.ToInteger8u) },
			{ typeof (short), new Converter<JSONValue, short>(DecoderConverter.ToInteger16s) },
			{ typeof (ushort), new Converter<JSONValue, ushort>(DecoderConverter.ToInteger16u) },
			{ typeof (int), new Converter<JSONValue, int>(DecoderConverter.ToInteger32s) },
			{ typeof (uint), new Converter<JSONValue, uint>(DecoderConverter.ToInteger32u) },
			{ typeof (long), new Converter<JSONValue, long>(DecoderConverter.ToInteger64s) },
			{ typeof (ulong), new Converter<JSONValue, ulong>(DecoderConverter.ToInteger64u) },
			{ typeof (string), new Converter<JSONValue, string>(DecoderConverter.ToString) },
			{ typeof (JSONValue), new Converter<JSONValue, JSONValue>((v) => v) }
		};

		public Converter<JSONValue, TTo> Get<TTo>()
		{
		    if (!this.converters.TryGetValue(typeof (TTo), out var box))
				throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "cannot convert JSON value into '{0}', please register a converter using schema's SetDecoderConverter method", typeof (TTo)));

			return (Converter<JSONValue, TTo>)box;
		}

		public void Set<TTo>(Converter<JSONValue, TTo> converter)
		{
            this.converters[typeof (TTo)] = converter ?? throw new ArgumentNullException(nameof(converter));
		}

		private static bool ToBoolean(JSONValue value)
		{
			switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean;

				case JSONType.Number:
					return Math.Abs(value.Number) >= double.Epsilon;

				case JSONType.String:
					return !string.IsNullOrEmpty(value.String);

				default:
					return false;
			}
		}

		private static char ToCharacter(JSONValue value)
		{
			switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? '1' : '\0';

				case JSONType.Number:
					// ReSharper disable once CompareOfFloatsByEqualityOperator
					return Math.Abs(value.Number) >= double.Epsilon ? '1' : '\0';

				case JSONType.String:
					return value.String.Length > 0 ? value.String[0] : '\0';

				default:
					return '\0';
			}
		}

		private static decimal ToDecimal(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? 1 : 0;

				case JSONType.Number:
					return value.NumberAsDecimal;

				case JSONType.String:
				    if (decimal.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static float ToFloat32(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? 1 : 0;

				case JSONType.Number:
					return (float)value.Number;

				case JSONType.String:
				    if (float.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static double ToFloat64(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? 1 : 0;

				case JSONType.Number:
					return value.Number;

				case JSONType.String:
					if (double.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static sbyte ToInteger8s(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? (sbyte)1 : (sbyte)0;

				case JSONType.Number:
					return (sbyte)value.Number;

				case JSONType.String:
					if (sbyte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static byte ToInteger8u(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? (byte)1 : (byte)0;

				case JSONType.Number:
					return (byte)value.Number;

				case JSONType.String:
					if (byte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static short ToInteger16s(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? (short)1 : (short)0;

				case JSONType.Number:
					return (short)value.Number;

				case JSONType.String:
					if (short.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ushort ToInteger16u(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? (ushort)1 : (ushort)0;

				case JSONType.Number:
					return (ushort)value.Number;

				case JSONType.String:
					if (ushort.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static int ToInteger32s(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? 1 : 0;

				case JSONType.Number:
					return (int)value.Number;

				case JSONType.String:
					if (int.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static uint ToInteger32u(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? 1u : 0;

				case JSONType.Number:
					return (uint)value.Number;

				case JSONType.String:
					if (uint.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static long ToInteger64s(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? 1 : 0;

				case JSONType.Number:
					return (long)value.Number;

				case JSONType.String:
					if (long.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ulong ToInteger64u(JSONValue value)
		{
		    switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? 1u : 0;

				case JSONType.Number:
					return (ulong)value.Number;

				case JSONType.String:
					if (ulong.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static string ToString(JSONValue value)
		{
			switch (value.Type)
			{
				case JSONType.Boolean:
					return value.Boolean ? "1" : string.Empty;

				case JSONType.Number:
					return value.Number.ToString(CultureInfo.InvariantCulture);

				case JSONType.String:
					return value.String;

				default:
					return string.Empty;
			}
		}
	}
}