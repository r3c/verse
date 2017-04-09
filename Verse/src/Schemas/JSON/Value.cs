using System;

namespace Verse.Schemas.JSON
{
    /// <summary>
    /// Native JSON value.
    /// </summary>
    public struct Value
    {
        #region Attributes / Instance

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
        public ContentType Type;

        #endregion

        #region Attributes / Static

        /// <summary>
        /// Static instance of undefined value.
        /// </summary>
        public static readonly Value Void = new Value();

        #endregion

        #region Methods

        /// <summary>
        /// Create a new boolean JSON value.
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <returns>JSON boolean value</returns>
        public static Value FromBoolean(bool value)
        {
            return new Value { Boolean = value, Type = ContentType.Boolean };
        }

        /// <summary>
        /// Create a new number JSON value.
        /// </summary>
        /// <param name="value">Number value</param>
        /// <returns>JSON number value</returns>
        public static Value FromNumber(decimal value)
        {
            return new Value { Number = value, Type = ContentType.Number };
        }

        /// <summary>
        /// Create a new string JSON value.
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>JSON string value</returns>
        public static Value FromString(string value)
        {
            return new Value { String = value, Type = ContentType.String };
        }

        #endregion
    }
}