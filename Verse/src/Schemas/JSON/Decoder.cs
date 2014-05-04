using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Decoder : IDecoder<Value>
	{
		#region Attributes

		private readonly Dictionary<Type, object>	converters = new Dictionary<Type, object>
		{
			{typeof (bool),		new Converter<Value, bool> (Decoder.ToBoolean)},
			{typeof (char),		new Converter<Value, char> (Decoder.ToCharacter)},
			{typeof (decimal),	new Converter<Value, decimal> (Decoder.ToDecimal)},
			{typeof (float),	new Converter<Value, float> (Decoder.ToFloat32)},
			{typeof (double),	new Converter<Value, double> (Decoder.ToFloat64)},
			{typeof (sbyte),	new Converter<Value, sbyte> (Decoder.ToInteger8s)},
			{typeof (byte),		new Converter<Value, byte> (Decoder.ToInteger8u)},
			{typeof (short),	new Converter<Value, short> (Decoder.ToInteger16s)},
			{typeof (ushort),	new Converter<Value, ushort> (Decoder.ToInteger16u)},
			{typeof (int),		new Converter<Value, int> (Decoder.ToInteger32s)},
			{typeof (uint),		new Converter<Value, uint> (Decoder.ToInteger32u)},
			{typeof (long),		new Converter<Value, long> (Decoder.ToInteger64s)},
			{typeof (ulong),	new Converter<Value, ulong> (Decoder.ToInteger64u)},
			{typeof (string),	new Converter<Value, string> (Decoder.ToString)},
			{typeof (Value),	new Converter<Value, Value> ((v) => v)}
		};

		#endregion

		#region Methods / Public

		public Converter<Value, T> Get<T> ()
		{
			object	box;

			if (!this.converters.TryGetValue (typeof (T), out box))
				throw new InvalidCastException (string.Format (CultureInfo.InvariantCulture, "no available converter from JSON value to type '{0}'", typeof (T)));

			return (Converter<Value, T>)box;
		}

		public void Set<T> (Converter<Value, T> converter)
		{
			if (converter == null)
				throw new ArgumentNullException ("converter");

			this.converters[typeof (T)] = converter;
		}

		#endregion

		#region Methods / Private

		private static bool ToBoolean (Value value)
		{
			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean;

				case Content.Number:
					return value.Number != 0;

				case Content.String:
					return !string.IsNullOrEmpty(value.String);

				default:
					return false;
			}
		}

		private static char ToCharacter (Value value)
		{
			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? '1' : '\0';

				case Content.Number:
					return value.Number != 0 ? '1' : '\0';

				case Content.String:
					return value.String.Length > 0 ? value.String[0] : '\0';

				default:
					return '\0';
			}
		}

		private static decimal ToDecimal (Value value)
		{
			decimal	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? 1 : 0;

				case Content.Number:
					return (decimal)value.Number;

				case Content.String:
					if (decimal.TryParse (value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static float ToFloat32 (Value value)
		{
			float	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? 1 : 0;

				case Content.Number:
					return (float)value.Number;

				case Content.String:
					if (float.TryParse (value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static double ToFloat64 (Value value)
		{
			double	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? 1 : 0;

				case Content.Number:
					return (double)value.Number;

				case Content.String:
					if (double.TryParse (value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static sbyte ToInteger8s (Value value)
		{
			sbyte	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? (sbyte)1 : (sbyte)0;

				case Content.Number:
					return (sbyte)value.Number;

				case Content.String:
					if (sbyte.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static byte ToInteger8u (Value value)
		{
			byte	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? (byte)1 : (byte)0;

				case Content.Number:
					return (byte)value.Number;

				case Content.String:
					if (byte.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static short ToInteger16s (Value value)
		{
			short	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? (short)1 : (short)0;

				case Content.Number:
					return (short)value.Number;

				case Content.String:
					if (short.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ushort ToInteger16u (Value value)
		{
			ushort	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? (ushort)1 : (ushort)0;

				case Content.Number:
					return (ushort)value.Number;

				case Content.String:
					if (ushort.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static int ToInteger32s (Value value)
		{
			int	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? 1 : 0;

				case Content.Number:
					return (int)value.Number;

				case Content.String:
					if (int.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static uint ToInteger32u (Value value)
		{
			uint	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? 1u : 0;

				case Content.Number:
					return (uint)value.Number;

				case Content.String:
					if (uint.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static long ToInteger64s (Value value)
		{
			long	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? 1 : 0;

				case Content.Number:
					return (long)value.Number;

				case Content.String:
					if (long.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ulong ToInteger64u (Value value)
		{
			ulong	number;

			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? 1u : 0;

				case Content.Number:
					return (ulong)value.Number;

				case Content.String:
					if (ulong.TryParse (value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static string ToString (Value value)
		{
			switch (value.Type)
			{
				case Content.Boolean:
					return value.Boolean ? "1" : string.Empty;

				case Content.Number:
					return value.Number.ToString (CultureInfo.InvariantCulture);

				case Content.String:
					return value.String;

				default:
					return string.Empty;
			}
		}

		#endregion
	}
}
