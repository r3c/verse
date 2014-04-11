using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
    class Adapter : IAdapter<Value>
    {
    	#region Attributes

    	private readonly Dictionary<Type, object>	converters = new Dictionary<Type, object>
    	{
    		{typeof (bool),		new Converter<Value, bool> (Adapter.ToBoolean)},
    		{typeof (char),		new Converter<Value, char> (Adapter.ToCharacter)},
    		{typeof (decimal),	new Converter<Value, decimal> (Adapter.ToDecimal)},
    		{typeof (float),	new Converter<Value, float> (Adapter.ToFloat32)},
    		{typeof (double),	new Converter<Value, double> (Adapter.ToFloat64)},
    		{typeof (sbyte),	new Converter<Value, sbyte> (Adapter.ToInteger8s)},
    		{typeof (byte),		new Converter<Value, byte> (Adapter.ToInteger8u)},
    		{typeof (short),	new Converter<Value, short> (Adapter.ToInteger16s)},
    		{typeof (ushort),	new Converter<Value, ushort> (Adapter.ToInteger16u)},
    		{typeof (int),		new Converter<Value, int> (Adapter.ToInteger32s)},
    		{typeof (uint),		new Converter<Value, uint> (Adapter.ToInteger32u)},
    		{typeof (long),		new Converter<Value, long> (Adapter.ToInteger64s)},
    		{typeof (ulong),	new Converter<Value, ulong> (Adapter.ToInteger64u)},
            {typeof (string),	new Converter<Value, string> (Adapter.ToString)}
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
			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean;

				case Content.Number:
					return value.number != 0;

				case Content.String:
					return !string.IsNullOrEmpty(value.str);

				default:
					return false;
			}
		}

		private static char ToCharacter (Value value)
		{
			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? '1' : '\0';

				case Content.Number:
					return value.number != 0 ? '1' : '\0';

				case Content.String:
					return value.str.Length > 0 ? value.str[0] : '\0';

				default:
					return '\0';
			}
		}

		private static decimal ToDecimal (Value value)
		{
			decimal	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? 1 : 0;

				case Content.Number:
					return (decimal)value.number;

				case Content.String:
					if (decimal.TryParse (value.str, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static float ToFloat32 (Value value)
		{
			float	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? 1 : 0;

				case Content.Number:
					return (float)value.number;

				case Content.String:
					if (float.TryParse (value.str, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static double ToFloat64 (Value value)
		{
			double	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? 1 : 0;

				case Content.Number:
					return (double)value.number;

				case Content.String:
					if (double.TryParse (value.str, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static sbyte ToInteger8s (Value value)
		{
			sbyte	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? (sbyte)1 : (sbyte)0;

				case Content.Number:
					return (sbyte)value.number;

				case Content.String:
					if (sbyte.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static byte ToInteger8u (Value value)
		{
			byte	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? (byte)1 : (byte)0;

				case Content.Number:
					return (byte)value.number;

				case Content.String:
					if (byte.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static short ToInteger16s (Value value)
		{
			short	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? (short)1 : (short)0;

				case Content.Number:
					return (short)value.number;

				case Content.String:
					if (short.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ushort ToInteger16u (Value value)
		{
			ushort	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? (ushort)1 : (ushort)0;

				case Content.Number:
					return (ushort)value.number;

				case Content.String:
					if (ushort.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static int ToInteger32s (Value value)
		{
			int	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? 1 : 0;

				case Content.Number:
					return (int)value.number;

				case Content.String:
					if (int.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static uint ToInteger32u (Value value)
		{
			uint	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? 1u : 0;

				case Content.Number:
					return (uint)value.number;

				case Content.String:
					if (uint.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static long ToInteger64s (Value value)
		{
			long	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? 1 : 0;

				case Content.Number:
					return (long)value.number;

				case Content.String:
					if (long.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static ulong ToInteger64u (Value value)
		{
			ulong	number;

			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? 1u : 0;

				case Content.Number:
					return (ulong)value.number;

				case Content.String:
					if (ulong.TryParse (value.str, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
						return number;

					return 0;

				default:
					return 0;
			}
		}

		private static string ToString (Value value)
		{
			switch (value.type)
			{
				case Content.Boolean:
					return value.boolean ? "1" : string.Empty;

				case Content.Number:
					return value.number.ToString (CultureInfo.InvariantCulture);

				case Content.String:
					return value.str;

				default:
					return string.Empty;
			}
		}

		#endregion
    }
}
