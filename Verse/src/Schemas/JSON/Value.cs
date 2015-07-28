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
        public double DecimalNumber;

        /// <summary>
        /// LongNumber value (only set if type is an integer number).
        /// </summary>
        public long LongNumber;

        /// <summary>
        /// String value (only set if type is string).
        /// </summary>
        public string String;

        /// <summary>
        /// Value content type.
        /// </summary>
        public Content Type;

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
            return new Value { Boolean = value, Type = Content.Boolean };
        }

        /// <summary>
        /// Create a new number JSON value.
        /// </summary>
        /// <param name="value">Number value</param>
        /// <returns>JSON number value</returns>
        public static Value FromNumber(double value)
        {
            return new Value { DecimalNumber = value, Type = Content.DecimalNumber };
        }

        /// <summary>
        /// Create a new number JSON value.
        /// </summary>
        /// <param name="value">Number value</param>
        /// <returns>JSON number value</returns>
        public static Value FromNumber(long value)
        {
            return new Value { LongNumber = value, Type = Content.LongNumber };
        }

        /// <summary>
        /// Create a new string JSON value.
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>JSON string value</returns>
        public static Value FromString(string value)
        {
            return new Value { String = value, Type = Content.String };
        }

        #endregion
    }
}