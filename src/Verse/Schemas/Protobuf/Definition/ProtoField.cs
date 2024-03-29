﻿namespace Verse.Schemas.Protobuf.Definition;

internal struct ProtoField
{
    public readonly string Name;

    public readonly int Number;

    public readonly ProtoOccurrence Occurrence;

    public readonly ProtoReference Reference;

    public ProtoField(int number, ProtoReference reference, string name, ProtoOccurrence occurrence)
    {
        Occurrence = occurrence;
        Number = number;
        Name = name;
        Reference = reference;
    }
}