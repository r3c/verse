using System;

namespace Verse.Schemas.Protobuf.Definition;

internal readonly struct Lexem(LexemType type, string value) : IEquatable<Lexem>
{
    public static readonly Lexem Edition = new(LexemType.Symbol, "edition");

    public static readonly Lexem Enum = new(LexemType.Symbol, "enum");

    public static readonly Lexem Extend = new(LexemType.Symbol, "extend");

    public static readonly Lexem Extensions = new(LexemType.Symbol, "extensions");

    public static readonly Lexem Group = new(LexemType.Symbol, "group");

    public static readonly Lexem Map = new(LexemType.Symbol, "map");

    public static readonly Lexem Max = new(LexemType.Symbol, "max");

    public static readonly Lexem Message = new(LexemType.Symbol, "message");

    public static readonly Lexem OneOf = new(LexemType.Symbol, "oneof");

    public static readonly Lexem Option = new(LexemType.Symbol, "option");

    public static readonly Lexem Optional = new(LexemType.Symbol, "optional");

    public static readonly Lexem Repeated = new(LexemType.Symbol, "repeated");

    public static readonly Lexem Required = new(LexemType.Symbol, "required");

    public static readonly Lexem Reserved = new(LexemType.Symbol, "reserved");

    public static readonly Lexem Syntax = new(LexemType.Symbol, "syntax");

    public static readonly Lexem To = new(LexemType.Symbol, "to");

    public readonly LexemType Type = type;

    public readonly string Value = value;

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

    public override bool Equals(object? obj)
    {
        return obj is Lexem lexem && Equals(lexem);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ Value.GetHashCode();
    }
}