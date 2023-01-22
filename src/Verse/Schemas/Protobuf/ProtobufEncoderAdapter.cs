using System;

namespace Verse.Schemas.Protobuf;

internal class ProtobufEncoderAdapter : IEncoderAdapter<ProtobufValue>
{
    public Func<bool, ProtobufValue> FromBoolean => v => new ProtobufValue(v);
    public Func<char, ProtobufValue> FromCharacter => v => new ProtobufValue(new string(v, 1));
    public Func<decimal, ProtobufValue> FromDecimal => v => new ProtobufValue((double) v);
    public Func<float, ProtobufValue> FromFloat32 => v => new ProtobufValue(v);
    public Func<double, ProtobufValue> FromFloat64 => v => new ProtobufValue(v);
    public Func<sbyte, ProtobufValue> FromInteger8S => v => new ProtobufValue(v);
    public Func<byte, ProtobufValue> FromInteger8U => v => new ProtobufValue(v);
    public Func<short, ProtobufValue> FromInteger16S => v => new ProtobufValue(v);
    public Func<ushort, ProtobufValue> FromInteger16U => v => new ProtobufValue(v);
    public Func<int, ProtobufValue> FromInteger32S => v => new ProtobufValue(v);
    public Func<uint, ProtobufValue> FromInteger32U => v => new ProtobufValue(v);
    public Func<long, ProtobufValue> FromInteger64S => v => new ProtobufValue(v);
    public Func<ulong, ProtobufValue> FromInteger64U => v => new ProtobufValue(v);
    public Func<string, ProtobufValue> FromString => v => new ProtobufValue(v);
}