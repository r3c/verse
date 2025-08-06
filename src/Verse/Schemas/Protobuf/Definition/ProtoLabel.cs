namespace Verse.Schemas.Protobuf.Definition;

internal struct ProtoLabel(int value, string name)
{
    public readonly string Name = name;

    public readonly int Value = value;
}