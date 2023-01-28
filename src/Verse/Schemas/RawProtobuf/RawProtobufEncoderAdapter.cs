using System;

namespace Verse.Schemas.RawProtobuf;

internal class RawProtobufEncoderAdapter : IEncoderAdapter<RawProtobufValue>
{
    public Func<bool, RawProtobufValue> Boolean =>
        v => new RawProtobufValue(v ? 1 : 0, RawProtobufWireType.VarInt);

    public Func<char, RawProtobufValue> Character => v =>
        new RawProtobufValue(new string(v, 1), RawProtobufWireType.VarInt);

    public unsafe Func<decimal, RawProtobufValue> Decimal => v =>
    {
        var number = (double) v;

        return new RawProtobufValue(*(long*) &number, RawProtobufWireType.Fixed64);
    };

    public unsafe Func<float, RawProtobufValue> Float32 =>
        v => new RawProtobufValue(*(int*) &v, RawProtobufWireType.Fixed32);

    public unsafe Func<double, RawProtobufValue> Float64 => v =>
        new RawProtobufValue(*(long*) &v, RawProtobufWireType.Fixed64);

    public Func<sbyte, RawProtobufValue> Integer8S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

    public Func<byte, RawProtobufValue> Integer8U => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

    public Func<short, RawProtobufValue> Integer16S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

    public Func<ushort, RawProtobufValue> Integer16U =>
        v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

    public Func<int, RawProtobufValue> Integer32S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

    public Func<uint, RawProtobufValue> Integer32U => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

    public Func<long, RawProtobufValue> Integer64S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

    public unsafe Func<ulong, RawProtobufValue> Integer64U =>
        v => new RawProtobufValue(*(long*) v, RawProtobufWireType.VarInt);

    public Func<string, RawProtobufValue> String => v => new RawProtobufValue(v, RawProtobufWireType.String);
}