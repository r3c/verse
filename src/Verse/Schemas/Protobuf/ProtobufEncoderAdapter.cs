using System;

namespace Verse.Schemas.Protobuf;

internal class ProtobufEncoderAdapter : IEncoderAdapter<ProtobufValue>
{
    public Func<bool, ProtobufValue> Boolean => v => new ProtobufValue(v);
    public Func<char, ProtobufValue> Character => v => new ProtobufValue(new string(v, 1));
    public Func<decimal, ProtobufValue> Decimal => v => new ProtobufValue((double) v);
    public Func<float, ProtobufValue> Float32 => v => new ProtobufValue(v);
    public Func<double, ProtobufValue> Float64 => v => new ProtobufValue(v);
    public Func<sbyte, ProtobufValue> Integer8S => v => new ProtobufValue(v);
    public Func<byte, ProtobufValue> Integer8U => v => new ProtobufValue(v);
    public Func<short, ProtobufValue> Integer16S => v => new ProtobufValue(v);
    public Func<ushort, ProtobufValue> Integer16U => v => new ProtobufValue(v);
    public Func<int, ProtobufValue> Integer32S => v => new ProtobufValue(v);
    public Func<uint, ProtobufValue> Integer32U => v => new ProtobufValue(v);
    public Func<long, ProtobufValue> Integer64S => v => new ProtobufValue(v);
    public Func<ulong, ProtobufValue> Integer64U => v => new ProtobufValue(v);
    public Func<string, ProtobufValue> String => v => new ProtobufValue(v);
}