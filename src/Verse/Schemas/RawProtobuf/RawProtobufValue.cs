namespace Verse.Schemas.RawProtobuf;

public readonly struct RawProtobufValue
{
    public readonly long Number;

    public readonly RawProtobufWireType Storage;

    public readonly string String;

    public RawProtobufValue(long number, RawProtobufWireType storage)
    {
        Number = number;
        Storage = storage;
        String = default;
    }

    public RawProtobufValue(string value, RawProtobufWireType storage)
    {
        Number = default;
        Storage = storage;
        String = value;
    }
}