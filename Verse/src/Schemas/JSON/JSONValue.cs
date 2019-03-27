
namespace Verse.Schemas.JSON
{
	/// <summary>
	/// Native JSON value.
	/// </summary>
	public struct JSONValue
	{
		/// <summary>
		/// Boolean value (only set if type is boolean).
		/// </summary>
		public bool Boolean;

		/// <summary>
		/// DecimalNumber value (only set if type is number).
		/// </summary>
		public decimal Number;

		/// <summary>
		/// String value (only set if type is string).
		/// </summary>
		public string String;

		/// <summary>
		/// Value content type.
		/// </summary>
		public JSONType Type;

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
			return new JSONValue { Boolean = value, Type = JSONType.Boolean };
		}

		/// <summary>
		/// Create a new number JSON value.
		/// </summary>
		/// <param name="value">Number value</param>
		/// <returns>JSON number value</returns>
		public static JSONValue FromNumber(decimal value)
		{
			return new JSONValue { Number = value, Type = JSONType.Number };
		}

		/// <summary>
		/// Create a new string JSON value.
		/// </summary>
		/// <param name="value">String value</param>
		/// <returns>JSON string value</returns>
		public static JSONValue FromString(string value)
		{
			return new JSONValue { String = value, Type = JSONType.String };
		}
	}
}