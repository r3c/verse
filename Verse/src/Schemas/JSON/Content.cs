using System;

namespace Verse.Schemas.JSON
{
    /// <summary>
    /// JSON native value content type.
    /// </summary>
    public enum Content
    {
        /// <summary>
        /// Undefined (null) value.
        /// </summary>
        Void,

        /// <summary>
        /// Boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Decimal number value.
        /// </summary>
        DecimalNumber,

        /// <summary>
        /// Long number value.
        /// </summary>
        LongNumber,

        /// <summary>
        /// String value.
        /// </summary>
        String
    }
}