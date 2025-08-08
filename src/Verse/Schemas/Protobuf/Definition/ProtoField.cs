namespace Verse.Schemas.Protobuf.Definition;

internal struct ProtoField(int number, ProtoReference reference, string name, ProtoPresence presence)
{
    public readonly string Name = name;

    public readonly int Number = number;

    public readonly ProtoPresence Presence = presence;

    public readonly ProtoReference Reference = reference;
}