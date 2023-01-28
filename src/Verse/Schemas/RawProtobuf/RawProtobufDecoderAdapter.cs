using System;
using System.Globalization;

namespace Verse.Schemas.RawProtobuf;

/// <summary>
/// Due to missing message type information when using "legacy" protobuf mode (only wire type is available) decoded
/// sources can only have 2 possible types: Signed & String. Converters will therefore trust caller for using the
/// correct type and perform reinterpret casts instead of actual conversions.
/// </summary>
internal class RawProtobufDecoderAdapter : IDecoderAdapter<RawProtobufValue>
{
    public Func<RawProtobufValue, bool> Boolean => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => source.Number != 0,
            RawProtobufWireType.Fixed64 => source.Number != 0,
            RawProtobufWireType.VarInt => source.Number != 0,
            RawProtobufWireType.String => !string.IsNullOrEmpty(source.String),
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, char> Character => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (char) source.Number,
            RawProtobufWireType.Fixed64 => (char) source.Number,
            RawProtobufWireType.VarInt => (char) source.Number,
            RawProtobufWireType.String => source.String.Length > 0 ? source.String[0] : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public unsafe Func<RawProtobufValue, decimal> Decimal => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (decimal) *(float*) &source.Number,
            RawProtobufWireType.Fixed64 => (decimal) *(double*) &source.Number,
            RawProtobufWireType.VarInt => source.Number,
            RawProtobufWireType.String => decimal.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public unsafe Func<RawProtobufValue, float> Float32 => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => *(float*) &source.Number,
            RawProtobufWireType.Fixed64 => (float) *(double*) &source.Number,
            RawProtobufWireType.VarInt => source.Number,
            RawProtobufWireType.String => float.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public unsafe Func<RawProtobufValue, double> Float64 => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => *(float*) &source.Number,
            RawProtobufWireType.Fixed64 => *(double*) &source.Number,
            RawProtobufWireType.VarInt => source.Number,
            RawProtobufWireType.String => double.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, sbyte> Integer8S => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (sbyte) source.Number,
            RawProtobufWireType.Fixed64 => (sbyte) source.Number,
            RawProtobufWireType.VarInt => (sbyte) source.Number,
            RawProtobufWireType.String => sbyte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, byte> Integer8U => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (byte) source.Number,
            RawProtobufWireType.Fixed64 => (byte) source.Number,
            RawProtobufWireType.VarInt => (byte) source.Number,
            RawProtobufWireType.String => byte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, short> Integer16S => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (short) source.Number,
            RawProtobufWireType.Fixed64 => (short) source.Number,
            RawProtobufWireType.VarInt => (short) source.Number,
            RawProtobufWireType.String => short.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, ushort> Integer16U => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (ushort) source.Number,
            RawProtobufWireType.Fixed64 => (ushort) source.Number,
            RawProtobufWireType.VarInt => (ushort) source.Number,
            RawProtobufWireType.String => ushort.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, int> Integer32S => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (int) source.Number,
            RawProtobufWireType.Fixed64 => (int) source.Number,
            RawProtobufWireType.VarInt => (int) source.Number,
            RawProtobufWireType.String => int.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, uint> Integer32U => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => (uint) source.Number,
            RawProtobufWireType.Fixed64 => (uint) source.Number,
            RawProtobufWireType.VarInt => (uint) source.Number,
            RawProtobufWireType.String => uint.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, long> Integer64S => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => source.Number,
            RawProtobufWireType.Fixed64 => source.Number,
            RawProtobufWireType.VarInt => source.Number,
            RawProtobufWireType.String => long.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public unsafe Func<RawProtobufValue, ulong> Integer64U => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => *(uint*) source.Number,
            RawProtobufWireType.Fixed64 => *(ulong*) source.Number,
            RawProtobufWireType.VarInt => (ulong) source.Number,
            RawProtobufWireType.String => ulong.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Storage), source.Storage, "invalid storage")
        };
    };

    public Func<RawProtobufValue, string> String => source =>
    {
        return source.Storage switch
        {
            RawProtobufWireType.Fixed32 => source.Number.ToString(CultureInfo.InvariantCulture),
            RawProtobufWireType.Fixed64 => source.Number.ToString(CultureInfo.InvariantCulture),
            RawProtobufWireType.VarInt => source.Number.ToString(CultureInfo.InvariantCulture),
            RawProtobufWireType.String => source.String,
            _ => string.Empty
        };
    };
}