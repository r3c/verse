using System.Globalization;

namespace Verse.Schemas.RawProtobuf;

/// <summary>
/// Due to missing message type information when using "legacy" protobuf mode (only wire type is available) decoded
/// sources can only have 2 possible types: Signed & String. Converters will therefore trust caller for using the
/// correct type and perform reinterpret casts instead of actual conversions.
/// </summary>
internal class RawProtobufDecoderAdapter : IDecoderAdapter<RawProtobufValue>
{
    public Setter<bool, RawProtobufValue> ToBoolean => (ref bool target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = source.Number != 0;

                break;

            case RawProtobufWireType.String:
                target = !string.IsNullOrEmpty(source.String);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<char, RawProtobufValue> ToCharacter => (ref char target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = (char) source.Number;

                break;

            case RawProtobufWireType.String:
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

    public unsafe Setter<decimal, RawProtobufValue> ToDecimal => (ref decimal target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
                target = (decimal) *(float*) &source.Number;

                break;

            case RawProtobufWireType.Fixed64:
                target = (decimal) *(double*) &source.Number;

                break;

            case RawProtobufWireType.VarInt:
                target = source.Number;

                break;

            case RawProtobufWireType.String:
                decimal.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public unsafe Setter<float, RawProtobufValue> ToFloat32 => (ref float target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
                target = *(float*) &source.Number;

                break;

            case RawProtobufWireType.Fixed64:
                target = (float) *(double*) &source.Number;

                break;

            case RawProtobufWireType.VarInt:
                target = source.Number;

                break;

            case RawProtobufWireType.String:
                float.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public unsafe Setter<double, RawProtobufValue> ToFloat64 => (ref double target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
                target = *(float*) &source.Number;

                break;

            case RawProtobufWireType.Fixed64:
                target = *(double*) &source.Number;

                break;

            case RawProtobufWireType.VarInt:
                target = source.Number;

                break;

            case RawProtobufWireType.String:
                double.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<sbyte, RawProtobufValue> ToInteger8S => (ref sbyte target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = (sbyte) source.Number;

                break;

            case RawProtobufWireType.String:
                sbyte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<byte, RawProtobufValue> ToInteger8U => (ref byte target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = (byte) source.Number;

                break;

            case RawProtobufWireType.String:
                byte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<short, RawProtobufValue> ToInteger16S => (ref short target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = (short) source.Number;

                break;

            case RawProtobufWireType.String:
                short.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<ushort, RawProtobufValue> ToInteger16U => (ref ushort target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = (ushort) source.Number;

                break;

            case RawProtobufWireType.String:
                ushort.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<int, RawProtobufValue> ToInteger32S => (ref int target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = (int) source.Number;

                break;

            case RawProtobufWireType.String:
                int.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<uint, RawProtobufValue> ToInteger32U => (ref uint target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = (uint) source.Number;

                break;

            case RawProtobufWireType.String:
                uint.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<long, RawProtobufValue> ToInteger64S => (ref long target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = source.Number;

                break;

            case RawProtobufWireType.String:
                long.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public unsafe Setter<ulong, RawProtobufValue> ToInteger64U => (ref ulong target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
                target = *(uint*) source.Number;

                break;

            case RawProtobufWireType.Fixed64:
                target = *(ulong*) source.Number;

                break;

            case RawProtobufWireType.VarInt:
                target = (ulong) source.Number;

                break;

            case RawProtobufWireType.String:
                ulong.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            default:
                target = default;

                break;
        }
    };

    public new Setter<string, RawProtobufValue> ToString => (ref string target, RawProtobufValue source) =>
    {
        switch (source.Storage)
        {
            case RawProtobufWireType.Fixed32:
            case RawProtobufWireType.Fixed64:
            case RawProtobufWireType.VarInt:
                target = source.Number.ToString(CultureInfo.InvariantCulture);

                break;

            case RawProtobufWireType.String:
                target = source.String;

                break;

            default:
                target = string.Empty;

                break;
        }
    };
}