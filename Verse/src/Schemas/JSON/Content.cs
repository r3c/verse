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
        Number,

        /// <summary>
        /// String value.
        /// </summary>
        String
    }
}