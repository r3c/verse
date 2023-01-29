using System;
using System.Globalization;

namespace Verse.Schemas.Protobuf;

internal class ProtobufDecoderAdapter : IDecoderAdapter<ProtobufValue>
{
    public Func<ProtobufValue, bool> Boolean => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean,
            ProtobufType.Float32 => Math.Abs(source.Float32) >= float.Epsilon,
            ProtobufType.Float64 => Math.Abs(source.Float64) >= float.Epsilon,
            ProtobufType.Signed => source.Signed != 0,
            ProtobufType.String => !string.IsNullOrEmpty(source.String),
            ProtobufType.Unsigned => source.Unsigned != 0,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, char> Character => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? '1' : '0',
            ProtobufType.Float32 => (char)source.Float32,
            ProtobufType.Float64 => (char)source.Float64,
            ProtobufType.Signed => (char)source.Signed,
            ProtobufType.String => source.String.Length > 0 ? source.String[0] : default,
            ProtobufType.Unsigned => (char)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, decimal> Decimal => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? 1 : 0,
            ProtobufType.Float32 => (decimal)source.Float32,
            ProtobufType.Float64 => (decimal)source.Float64,
            ProtobufType.Signed => source.Signed,
            ProtobufType.String => decimal.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, float> Float32 => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? 1 : 0,
            ProtobufType.Float32 => source.Float32,
            ProtobufType.Float64 => (float)source.Float64,
            ProtobufType.Signed => source.Signed,
            ProtobufType.String => float.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, double> Float64 => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? 1 : 0,
            ProtobufType.Float32 => source.Float32,
            ProtobufType.Float64 => source.Float64,
            ProtobufType.Signed => source.Signed,
            ProtobufType.String => double.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, sbyte> Integer8S => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? (sbyte)1 : (sbyte)0,
            ProtobufType.Float32 => (sbyte)source.Float32,
            ProtobufType.Float64 => (sbyte)source.Float64,
            ProtobufType.Signed => (sbyte)source.Signed,
            ProtobufType.String => sbyte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => (sbyte)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, byte> Integer8U => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? (byte)1 : (byte)0,
            ProtobufType.Float32 => (byte)source.Float32,
            ProtobufType.Float64 => (byte)source.Float64,
            ProtobufType.Signed => (byte)source.Signed,
            ProtobufType.String => byte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => (byte)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, short> Integer16S => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? (short)1 : (short)0,
            ProtobufType.Float32 => (short)source.Float32,
            ProtobufType.Float64 => (short)source.Float64,
            ProtobufType.Signed => (short)source.Signed,
            ProtobufType.String => short.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => (short)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, ushort> Integer16U => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? (ushort)1 : (ushort)0,
            ProtobufType.Float32 => (ushort)source.Float32,
            ProtobufType.Float64 => (ushort)source.Float64,
            ProtobufType.Signed => (ushort)source.Signed,
            ProtobufType.String => ushort.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => (ushort)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, int> Integer32S => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? 1 : 0,
            ProtobufType.Float32 => (int)source.Float32,
            ProtobufType.Float64 => (int)source.Float64,
            ProtobufType.Signed => (int)source.Signed,
            ProtobufType.String => int.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => (int)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, uint> Integer32U => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? 1u : 0u,
            ProtobufType.Float32 => (uint)source.Float32,
            ProtobufType.Float64 => (uint)source.Float64,
            ProtobufType.Signed => (uint)source.Signed,
            ProtobufType.String => uint.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => (uint)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, long> Integer64S => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? 1 : 0,
            ProtobufType.Float32 => (long)source.Float32,
            ProtobufType.Float64 => (long)source.Float64,
            ProtobufType.Signed => source.Signed,
            ProtobufType.String => long.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => (long)source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, ulong> Integer64U => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? 1u : 0u,
            ProtobufType.Float32 => (ulong)source.Float32,
            ProtobufType.Float64 => (ulong)source.Float64,
            ProtobufType.Signed => (ulong)source.Signed,
            ProtobufType.String => ulong.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target)
                ? target
                : default,
            ProtobufType.Unsigned => source.Unsigned,
            ProtobufType.Void => default,
            _ => throw new ArgumentOutOfRangeException(nameof(source.Type), source.Type, "invalid type")
        };
    };

    public Func<ProtobufValue, string> String => source =>
    {
        return source.Type switch
        {
            ProtobufType.Boolean => source.Boolean ? "1" : string.Empty,
            ProtobufType.Float32 => source.Float32.ToString(CultureInfo.InvariantCulture),
            ProtobufType.Float64 => source.Float64.ToString(CultureInfo.InvariantCulture),
            ProtobufType.Signed => source.Signed.ToString(CultureInfo.InvariantCulture),
            ProtobufType.String => source.String,
            ProtobufType.Unsigned => source.Unsigned.ToString(CultureInfo.InvariantCulture),
            ProtobufType.Void => string.Empty,
            _ => string.Empty
        };
    };
}