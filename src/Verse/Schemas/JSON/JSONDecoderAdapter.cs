using System;
using System.Globalization;

namespace Verse.Schemas.JSON;

internal class JSONDecoderAdapter : IDecoderAdapter<JSONValue>
{
    public Setter<bool, JSONValue> ToBoolean => (ref bool target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean;

                break;

            case JSONType.Number:
                target = Math.Abs(source.Number) >= double.Epsilon;

                break;

            case JSONType.String:
                target = !string.IsNullOrEmpty(source.String);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<char, JSONValue> ToCharacter => (ref char target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? '1' : '\0';

                break;

            case JSONType.Number:
                target = Math.Abs(source.Number) >= double.Epsilon ? '1' : '\0';

                break;

            case JSONType.String:
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

    public Setter<decimal, JSONValue> ToDecimal => (ref decimal target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JSONType.Number:
                target = (decimal)source.Number;

                break;

            case JSONType.String:
                decimal.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<float, JSONValue> ToFloat32 => (ref float target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JSONType.Number:
                target = (float) source.Number;

                break;

            case JSONType.String:
                float.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<double, JSONValue> ToFloat64 => (ref double target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JSONType.Number:
                target = source.Number;

                break;

            case JSONType.String:
                double.TryParse(source.String, NumberStyles.Float, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<sbyte, JSONValue> ToInteger8S => (ref sbyte target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? (sbyte) 1 : (sbyte) 0;

                break;

            case JSONType.Number:
                target = (sbyte) source.Number;

                break;

            case JSONType.String:
                sbyte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<byte, JSONValue> ToInteger8U => (ref byte target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? (byte) 1 : (byte) 0;

                break;

            case JSONType.Number:
                target = (byte) source.Number;

                break;

            case JSONType.String:
                byte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<short, JSONValue> ToInteger16S => (ref short target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? (short) 1 : (short) 0;

                break;

            case JSONType.Number:
                target = (short) source.Number;

                break;

            case JSONType.String:
                short.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<ushort, JSONValue> ToInteger16U => (ref ushort target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? (ushort) 1 : (ushort) 0;

                break;

            case JSONType.Number:
                target = (ushort) source.Number;

                break;

            case JSONType.String:
                ushort.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<int, JSONValue> ToInteger32S => (ref int target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JSONType.Number:
                target = (int) source.Number;

                break;

            case JSONType.String:
                int.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<uint, JSONValue> ToInteger32U => (ref uint target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? 1u : 0;

                break;

            case JSONType.Number:
                target = (uint) source.Number;

                break;

            case JSONType.String:
                uint.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<long, JSONValue> ToInteger64S => (ref long target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case JSONType.Number:
                target = (long) source.Number;

                break;

            case JSONType.String:
                long.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<ulong, JSONValue> ToInteger64U => (ref ulong target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? 1u : 0;

                break;

            case JSONType.Number:
                target = (ulong) source.Number;

                break;

            case JSONType.String:
                ulong.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public new Setter<string, JSONValue> ToString => (ref string target, JSONValue source) =>
    {
        switch (source.Type)
        {
            case JSONType.Boolean:
                target = source.Boolean ? "1" : string.Empty;

                break;

            case JSONType.Number:
                target = source.Number.ToString(CultureInfo.InvariantCulture);

                break;

            case JSONType.String:
                target = source.String;

                break;

            default:
                target = default;

                break;
        }
    };
}