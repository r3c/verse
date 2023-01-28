using System;
using System.Globalization;

namespace Verse.Schemas.Json;

internal class JsonDecoderAdapter : IDecoderAdapter<JsonValue>
{
    public Func<JsonValue, bool> Boolean => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean,
            JsonType.Number => Math.Abs(source.Number) >= double.Epsilon,
            JsonType.String => !string.IsNullOrEmpty(source.String),
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, char> Character => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? '1' : '\0',
            JsonType.Number => Math.Abs(source.Number) >= double.Epsilon ? '1' : '\0',
            JsonType.String => source.String.Length > 0 ? source.String[0] : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, decimal> Decimal => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? 1 : 0,
            JsonType.Number => (decimal) source.Number,
            JsonType.String => decimal.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, float> Float32 => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? 1 : 0,
            JsonType.Number => (float) source.Number,
            JsonType.String => float.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, double> Float64 => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? 1 : 0,
            JsonType.Number => source.Number,
            JsonType.String => double.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, sbyte> Integer8S => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? (sbyte) 1 : (sbyte) 0,
            JsonType.Number => (sbyte) source.Number,
            JsonType.String => sbyte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, byte> Integer8U => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? (byte) 1 : (byte) 0,
            JsonType.Number => (byte) source.Number,
            JsonType.String => byte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, short> Integer16S => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? (short) 1 : (short) 0,
            JsonType.Number => (short) source.Number,
            JsonType.String => short.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, ushort> Integer16U => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? (ushort) 1 : (ushort) 0,
            JsonType.Number => (ushort) source.Number,
            JsonType.String => ushort.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, int> Integer32S => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? 1 : 0,
            JsonType.Number => (int) source.Number,
            JsonType.String => int.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            JsonType.Undefined => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, uint> Integer32U => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? 1u : 0,
            JsonType.Number => (uint) source.Number,
            JsonType.String => uint.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, long> Integer64S => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? 1 : 0,
            JsonType.Number => (long) source.Number,
            JsonType.String => long.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, ulong> Integer64U => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? 1u : 0,
            JsonType.Number => (ulong) source.Number,
            JsonType.String => ulong.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<JsonValue, string> String => source =>
    {
        return source.Type switch
        {
            JsonType.Boolean => source.Boolean ? "1" : string.Empty,
            JsonType.Number => source.Number.ToString(CultureInfo.InvariantCulture),
            JsonType.String => source.String,
            _ => string.Empty
        };
    };
}