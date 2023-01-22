using System;

namespace Verse.Schemas.Protobuf.Definition;

internal struct Lexem : IEquatable<Lexem>
{
    public static readonly Lexem Enum = new Lexem(LexemType.Symbol, "enum");

    public static readonly Lexem Extend = new Lexem(LexemType.Symbol, "extend");

    public static readonly Lexem Extensions = new Lexem(LexemType.Symbol, "extensions");

    public static readonly Lexem Group = new Lexem(LexemType.Symbol, "group");

    public static readonly Lexem Map = new Lexem(LexemType.Symbol, "map");

    public static readonly Lexem Message = new Lexem(LexemType.Symbol, "message");

    public static readonly Lexem OneOf = new Lexem(LexemType.Symbol, "oneof");

    public static readonly Lexem Option = new Lexem(LexemType.Symbol, "option");

    public static readonly Lexem Optional = new Lexem(LexemType.Symbol, "optional");

    public static readonly Lexem Repeated = new Lexem(LexemType.Symbol, "repeated");

    public static readonly Lexem Required = new Lexem(LexemType.Symbol, "required");

    public static readonly Lexem Reserved = new Lexem(LexemType.Symbol, "reserved");

    public readonly LexemType Type;

    public readonly string Value;

    public Lexem(LexemType type, string value)
    {
        Type = type;
        Value = value;
    }

    public static bool operator ==(Lexem lhs, Lexem rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Lexem lhs, Lexem rhs)
    {
        return !lhs.Equals(rhs);
    }

    public bool Equals(Lexem other)
    {
        return Type == other.Type && Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return (obj is Lexem lexem) && Equals(lexem);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ Value.GetHashCode();
    }
}