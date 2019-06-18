using System;

namespace Verse.Schemas.JSON
{
	/// <summary>
	/// Native JSON value.
	/// </summary>
	public readonly struct JSONValue
	{
		/// <summary>
		/// Boolean value (only set if type is boolean).
		/// </summary>
		public readonly bool Boolean;

		/// <summary>
		/// Numeric value (only set if type is number).
		/// </summary>
		public double Number => this.numberMantissa * this.number10PowExponent;

		/// <summary>
		/// Numeric decimal value (only set if type is number).
		/// </summary>
		public decimal NumberAsDecimal => this.numberMantissa * (decimal) this.number10PowExponent;

		private readonly long numberMantissa;
		private readonly double number10PowExponent;

		/// <summary>
		/// String value (only set if type is string).
		/// </summary>
		public readonly string String;

		/// <summary>
		/// Value content type.
		/// </summary>
		public readonly JSONType Type;

		/// <summary>
		/// Static instance of undefined value.
		/// </summary>
		public static readonly JSONValue Void = new JSONValue();

		/// <summary>
		/// Create a new boolean JSON value.
		/// </summary>
		/// <param name="value">Boolean value</param>
		/// <returns>JSON boolean value</returns>
		public static JSONValue FromBoolean(bool value)
		{
			return new JSONValue(JSONType.Boolean, value, default, default, default);
		}

		/// <summary>
		/// Create a new number JSON value.
		/// </summary>
		/// <param name="value">Number value</param>
		/// <returns>JSON number value</returns>
		public static JSONValue FromNumber(double number)
		{
			if (double.IsInfinity(number) || double.IsNaN(number))
			{
				return new JSONValue(JSONType.Number, default, 1, number, default);
			}
			else
			{
				var exponent = JSONValue.ComputeExponent(number);
				var number10PowExponent = Math.Pow(10d, exponent);
				if (number10PowExponent == 0 && exponent < 0)
				{
					number10PowExponent = double.Epsilon;
				}
				var mantissa = (long) (number / number10PowExponent);

				var res = new JSONValue(JSONType.Number, default, mantissa, number10PowExponent, default);
				return res;
			}
		}

		private static int ComputeExponent(double value)
		{
			// Extract the exponent from the double:
			// Sign (s, 1 bit) Stored exponent (e, 11 bits) Mantissa (m, 52 bits)
			// with value = (-1)s x 1.m (binary) x 2e-1023.
			var bits = BitConverter.DoubleToInt64Bits(value);
			// Get the biased exponent in base 2
			var e = (int)((bits >> 52) & 0x7ffL);

			// Bias the exponent. It's actually biased by 1023.
			e -= 1023;

			// We want the exponent according to a full integer mantissa.
			e -= 52;

			// compute exponent in base 10 (from base 2)
			return (int)Math.Round(e / Math.Log(10, 2));
		}

		/// <summary>
		/// Create a new number JSON value.
		/// </summary>
		/// <param name="numberMantissa">Number mantissa value</param>
		/// <param name="numberExponent">Number exponent value (in base 10)</param>
		/// <returns>JSON number value</returns>
		public static JSONValue FromNumber(long numberMantissa, int numberExponent)
		{
			return new JSONValue(JSONType.Number, default, numberMantissa, Math.Pow(10, numberExponent), default);
		}

		/// <summary>
		/// Create a new string JSON value.
		/// </summary>
		/// <param name="value">String value</param>
		/// <returns>JSON string value</returns>
		public static JSONValue FromString(string value)
		{
			return value == null ? JSONValue.Void : new JSONValue(JSONType.String, default, default, default, value);
		}

		private JSONValue(JSONType type, bool boolean, long numberMantissa, double number10PowExponent, string str)
		{
			this.Boolean = boolean;
			this.numberMantissa = numberMantissa;
			this.number10PowExponent = number10PowExponent;
			this.String = str;
			this.Type = type;
		}
	}
}