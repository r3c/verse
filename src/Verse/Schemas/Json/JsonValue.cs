namespace Verse.Schemas.Json;

/// <summary>
/// Native JSON value.
/// </summary>
public readonly struct JsonValue
{
    /// <summary>
    /// Boolean value (only set if type is boolean).
    /// </summary>
    public readonly bool Boolean;

    /// <summary>
    /// Numeric value (only set if type is number).
    /// </summary>
    public readonly double Number;

    /// <summary>
    /// String value (only set if type is string).
    /// </summary>
    public readonly string String;

    /// <summary>
    /// Value content type.
    /// </summary>
    public readonly JsonType Type;

    /// <summary>
    /// Static instance of undefined value.
    /// </summary>
    public static readonly JsonValue Undefined = new();

    /// <summary>
    /// Create a new boolean JSON value.
    /// </summary>
    /// <param name="value">Boolean value</param>
    /// <returns>JSON boolean value</returns>
    public static JsonValue FromBoolean(bool value)
    {
        return new JsonValue(JsonType.Boolean, value, default, default);
    }

    /// <summary>
    /// Create a new number JSON value.
    /// </summary>
    /// <param name="value">Number value</param>
    /// <returns>JSON number value</returns>
    public static JsonValue FromNumber(double value)
    {
        return new JsonValue(JsonType.Number, default, value, default);
    }

    /// <summary>
    /// Create a new string JSON value.
    /// </summary>
    /// <param name="value">String value</param>
    /// <returns>JSON string value</returns>
    public static JsonValue FromString(string value)
    {
        return value == null ? Undefined : new JsonValue(JsonType.String, default, default, value);
    }

    private JsonValue(JsonType type, bool boolean, double number, string str)
    {
        Boolean = boolean;
        Number = number;
        String = str;
        Type = type;
    }
}