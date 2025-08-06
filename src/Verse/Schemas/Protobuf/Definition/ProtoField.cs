namespace Verse.Schemas.Protobuf.Definition;

internal struct ProtoField(int number, ProtoReference reference, string name, ProtoOccurrence occurrence)
{
    public readonly string Name = name;

    public readonly int Number = number;

    public readonly ProtoOccurrence Occurrence = occurrence;

    public readonly ProtoReference Reference = reference;
}