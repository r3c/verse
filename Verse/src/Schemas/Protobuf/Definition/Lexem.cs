using System;

namespace Verse.Schemas.Protobuf.Definition
{
    struct Lexem : IEquatable<Lexem>
    {
        #region Attributes / Static

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

        #endregion

        #region Attributes / Instance

        public readonly LexemType Type;

        public readonly string Value;

        #endregion

        #region Constructors

        public Lexem(LexemType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        #endregion

        #region Operators

        public static bool operator ==(Lexem lhs, Lexem rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Lexem lhs, Lexem rhs)
        {
            return !lhs.Equals(rhs);
        }

        #endregion

        #region Methods

        public bool Equals(Lexem other)
        {
            return this.Type == other.Type && this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return (obj is Lexem) && this.Equals((Lexem)obj);
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() ^ this.Value.GetHashCode();
        }

        #endregion
    }
}
