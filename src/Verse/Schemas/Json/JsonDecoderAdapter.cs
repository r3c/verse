using System;
using System.Globalization;

namespace Verse.Schemas.Json;

internal class JsonDecoderAdapter : IDecoderAdapter<JsonValue>
{
    public Setter<bool, JsonValue> Boolean => (ref bool target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean;

                break;

            case JsonType.Number:
                target = Math.Abs(source.Number) >= double.Epsilon;

                break;

            case JsonType.String:
                target = !string.IsNullOrEmpty(source.String);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<char, JsonValue> Character => (ref char target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? '1' : '\0';

                break;

            case JsonType.Number:
                target = Math.Abs(source.Number) >= double.Epsilon ? '1' : '\0';

                break;

            case JsonType.String:
                if (source.String.Length > 0)
                {
                    target = source.String[0];

                    break;
                }

                target = default;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<decimal, JsonValue> Decimal => (ref decimal target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JsonType.Number:
                target = (decimal)source.Number;

                break;

            case JsonType.String:
                decimal.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<float, JsonValue> Float32 => (ref float target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JsonType.Number:
                target = (float) source.Number;

                break;

            case JsonType.String:
                float.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<double, JsonValue> Float64 => (ref double target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JsonType.Number:
                target = source.Number;

                break;

            case JsonType.String:
                double.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<sbyte, JsonValue> Integer8S => (ref sbyte target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? (sbyte) 1 : (sbyte) 0;

                break;

            case JsonType.Number:
                target = (sbyte) source.Number;

                break;

            case JsonType.String:
                sbyte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<byte, JsonValue> Integer8U => (ref byte target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? (byte) 1 : (byte) 0;

                break;

            case JsonType.Number:
                target = (byte) source.Number;

                break;

            case JsonType.String:
                byte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<short, JsonValue> Integer16S => (ref short target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? (short) 1 : (short) 0;

                break;

            case JsonType.Number:
                target = (short) source.Number;

                break;

            case JsonType.String:
                short.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<ushort, JsonValue> Integer16U => (ref ushort target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? (ushort) 1 : (ushort) 0;

                break;

            case JsonType.Number:
                target = (ushort) source.Number;

                break;

            case JsonType.String:
                ushort.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<int, JsonValue> Integer32S => (ref int target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JsonType.Number:
                target = (int) source.Number;

                break;

            case JsonType.String:
                int.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<uint, JsonValue> Integer32U => (ref uint target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? 1u : 0;

                break;

            case JsonType.Number:
                target = (uint) source.Number;

                break;

            case JsonType.String:
                uint.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<long, JsonValue> Integer64S => (ref long target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JsonType.Number:
                target = (long) source.Number;

                break;

            case JsonType.String:
                long.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<ulong, JsonValue> Integer64U => (ref ulong target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? 1u : 0;

                break;

            case JsonType.Number:
                target = (ulong) source.Number;

                break;

            case JsonType.String:
                ulong.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<string, JsonValue> String => (ref string target, JsonValue source) =>
    {
        switch (source.Type)
        {
            case JsonType.Boolean:
                target = source.Boolean ? "1" : string.Empty;

                break;

            case JsonType.Number:
                target = source.Number.ToString(CultureInfo.InvariantCulture);

                break;

            case JsonType.String:
                target = source.String;

                break;

            default:
                target = string.Empty;

                break;
        }
    };
}