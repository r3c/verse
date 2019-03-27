using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;

namespace Verse.Schemas.Protobuf.Definition
{
    /// <summary>
    /// Protocol Buffers specification parser for proto 2 and 3 specifications:
    /// https://developers.google.com/protocol-buffers/docs/reference/proto3-spec
    /// To be implemented:
    /// - oct and hex literal numbers
    /// - reserved fields
    /// - extensions
    /// - services
    /// - options
    /// - import
    /// - package
    /// - syntax
    /// Not implemented:
    /// - groups
    /// </summary>
    static class Parser
    {
        public static ProtoEntity Parse(TextReader proto)
        {
            var entity = new ProtoEntity(ProtoContainer.Root, string.Empty);
            var lexer = new Lexer(proto);

            while (lexer.Current.Type != LexemType.End)
            {
                if (lexer.Current == Lexem.Option)
                {
                    lexer.Next();

                    Parser.ParseOption(lexer);
                    Parser.ParseValue(lexer, LexemType.SemiColon, "semicolon");
                }
                else
                    entity.Entities.Add(Parser.ParseEntity(lexer));
            }

            return entity;
        }

        private static ProtobufValue ParseConstant(Lexer lexer)
        {
            ProtobufValue value;

            switch (lexer.Current.Type)
            {
                case LexemType.Minus:
                    lexer.Next();

                    if (lexer.Current.Type == LexemType.Number)
                        return Parser.ParseNumber(lexer, -1);

                    break;

                case LexemType.Number:
                    return Parser.ParseNumber(lexer, 1);

                case LexemType.Plus:
                    lexer.Next();

                    if (lexer.Current.Type == LexemType.Number)
                        return Parser.ParseNumber(lexer, 1);

                    break;

                case LexemType.String:
                    return new ProtobufValue(Parser.ParseValue(lexer, LexemType.String, string.Empty));

                case LexemType.Symbol:
                    if (lexer.Current.Value == "false")
                        value = new ProtobufValue(false);
                    else if (lexer.Current.Value == "true")
                        value = new ProtobufValue(true);
                    else
                        break;

                    lexer.Next();

                    return value;
            }

            throw new ParserException(lexer.Position, "expected constant value");
        }

        private static ProtoEntity ParseEntity(Lexer lexer)
        {
            if (lexer.Current.Type == LexemType.Symbol)
            {
                var value = lexer.Current.Value;

                lexer.Next();

                if (value == "enum")
                    return Parser.ParseEnum(lexer);
                else if (value == "message")
                    return Parser.ParseMessage(lexer, new string[0]);
            }

            throw new ParserException(lexer.Position, "expected enum or message definition");
        }

        private static ProtoEntity ParseEnum(Lexer lexer)
        {
            var entity = new ProtoEntity(ProtoContainer.Enum, Parser.ParseValue(lexer, LexemType.Symbol, "enum name"));

            Parser.ParseValue(lexer, LexemType.BraceBegin, "opening brace");

            while (lexer.Current.Type != LexemType.BraceEnd)
            {
                var label = Parser.ParseValue(lexer, LexemType.Symbol, "enum label");

                if (label == "option")
                    Parser.ParseOption(lexer);
                else
                {
                    Parser.ParseValue(lexer, LexemType.Equal, "equal sign");

                    int value;
                    if (!int.TryParse(Parser.ParseValue(lexer, LexemType.Number, "enum value"), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                        throw new ParserException(lexer.Position, "invalid enum value");

                    if (lexer.Current.Type == LexemType.BracketBegin)
                    {
                        lexer.Next();

                        Parser.ParseOptions(lexer);
                        Parser.ParseValue(lexer, LexemType.BracketEnd, "closing bracket");
                    }

                    entity.Labels.Add(new ProtoLabel(value, label));
                }

                Parser.ParseValue(lexer, LexemType.SemiColon, "semicolon");
            }

            lexer.Next();

            if (entity.Labels.Count < 1 || entity.Labels[0].Value != 0)
                throw new ParserException(lexer.Position, "first enum value must be zero");

            return entity;
        }

        private static ProtoField ParseField(Lexer lexer, ProtoReference reference, ProtoOccurrence occurrence)
        {
            string name = Parser.ParseValue(lexer, LexemType.Symbol, "field name");

            Parser.ParseValue(lexer, LexemType.Equal, "equal sign");

            if (!int.TryParse(Parser.ParseValue(lexer, LexemType.Number, "field number"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                throw new ParserException(lexer.Position, "invalid field number");

            if (lexer.Current.Type == LexemType.BracketBegin)
            {
                lexer.Next();

                Parser.ParseOptions(lexer);
                Parser.ParseValue(lexer, LexemType.BracketEnd, "closing bracket");
            }

            return new ProtoField(number, reference, name, occurrence);
        }

        private static string ParseFullIdent(Lexer lexer)
        {
            var name = string.Empty;

            do
            {
                if (name.Length > 0)
                    name += ".";

                name += Parser.ParseValue(lexer, LexemType.Symbol, "identifier");

                if (lexer.Current.Type != LexemType.Dot)
                    break;

                lexer.Next();
            }
            while (true);

            return name;
        }

        private static Tuple<ProtoEntity, ProtoField> ParseMapField(Lexer lexer, IEnumerable<string> parentNames)
        {
            var entity = new ProtoEntity(ProtoContainer.Message, Guid.NewGuid().ToString("N"));

            Parser.ParseValue(lexer, LexemType.LowerThan, "lower than sign");

            entity.Fields.Add(new ProtoField(1, Parser.ParseType(lexer, parentNames), "key", ProtoOccurrence.Required));

            Parser.ParseValue(lexer, LexemType.Comma, "comma");

            entity.Fields.Add(new ProtoField(2, Parser.ParseType(lexer, parentNames), "value", ProtoOccurrence.Required));

            Parser.ParseValue(lexer, LexemType.GreaterThan, "greater than sign");

            return Tuple.Create(entity, Parser.ParseField(lexer, new ProtoReference(parentNames.Concat(new [] { entity.Name })), ProtoOccurrence.Required));
        }

        private static ProtoEntity ParseMessage(Lexer lexer, IEnumerable<string> parentNames)
        {
            var name = Parser.ParseValue(lexer, LexemType.Symbol, "message name");

            var currentNames = parentNames.Concat(new [] { name });
            var entity = new ProtoEntity(ProtoContainer.Message, name);

            Parser.ParseValue(lexer, LexemType.BraceBegin, "opening brace");

            while (lexer.Current.Type != LexemType.BraceEnd)
            {
                if (lexer.Current == Lexem.Enum)
                {
                    lexer.Next();

                    entity.Entities.Add(Parser.ParseEnum(lexer));
                }
                else if (lexer.Current == Lexem.Extend)
                    throw new InvalidOperationException("extensions are not supported yet");
                else if (lexer.Current == Lexem.Extensions)
                    throw new InvalidOperationException("extensions are not supported yet");
                else if (lexer.Current == Lexem.Group)
                    throw new InvalidOperationException("groups are not supported yet");
                else if (lexer.Current == Lexem.Map)
                {
                    lexer.Next();

                    var map = Parser.ParseMapField(lexer, currentNames);

                    entity.Entities.Add(map.Item1);
                    entity.Fields.Add(map.Item2);

                    Parser.ParseValue(lexer, LexemType.SemiColon, "semicolon");
                }
                else if (lexer.Current == Lexem.Message)
                {
                    lexer.Next();

                    entity.Entities.Add(Parser.ParseMessage(lexer, currentNames));
                }
                else if (lexer.Current == Lexem.OneOf)
                {
                    lexer.Next();

                    entity.Entities.Add(Parser.ParseOneOf(lexer, currentNames));
                }
                else if (lexer.Current == Lexem.Option)
                {
                    lexer.Next();

                    Parser.ParseOption(lexer);
                    Parser.ParseValue(lexer, LexemType.SemiColon, "semicolon");
                }
                else if (lexer.Current == Lexem.Reserved)
                    throw new InvalidOperationException("reserved fields are not supported yet");
                else
                {
                    ProtoOccurrence occurrence;

                    if (lexer.Current == Lexem.Optional)
                    {
                        lexer.Next();

                        occurrence = ProtoOccurrence.Optional;
                    }
                    else if (lexer.Current == Lexem.Repeated)
                    {
                        lexer.Next();

                        occurrence = ProtoOccurrence.Repeated;
                    }
                    else if (lexer.Current == Lexem.Required)
                    {
                        lexer.Next();

                        occurrence = ProtoOccurrence.Required;
                    }
                    else
                        occurrence = ProtoOccurrence.Optional;

                    entity.Fields.Add(Parser.ParseField(lexer, Parser.ParseType(lexer, parentNames), occurrence));

                    Parser.ParseValue(lexer, LexemType.SemiColon, "semicolon");
                }
            }

            lexer.Next();

            return entity;
        }

        private static ProtobufValue ParseNumber(Lexer lexer, int sign)
        {
            string number = lexer.Current.Value;

            lexer.Next();

            // FIXME: oct and hex literals not handled
            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInteger))
                return new ProtobufValue(sign * asInteger);
            else if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var asDecimal))
                return new ProtobufValue(sign * asDecimal);
            else
                throw new ParserException(lexer.Position, "invalid literal number");
        }

        private static ProtoEntity ParseOneOf(Lexer lexer, IEnumerable<string> parentNames)
        {
            var entity = new ProtoEntity(ProtoContainer.OneOf, Parser.ParseValue(lexer, LexemType.Symbol, "oneof name"));

            Parser.ParseValue(lexer, LexemType.BraceBegin, "opening brace");

            while (lexer.Current.Type != LexemType.BraceEnd)
            {
                entity.Fields.Add(Parser.ParseField(lexer, Parser.ParseType(lexer, parentNames), ProtoOccurrence.Required));

                Parser.ParseValue(lexer, LexemType.SemiColon, "semicolon");
            }

            lexer.Next();

            return entity;
        }

        private static void ParseOption(Lexer lexer)
        {
            if (lexer.Current.Type == LexemType.ParenthesisBegin)
            {
                lexer.Next();

                Parser.ParseFullIdent(lexer);
                Parser.ParseValue(lexer, LexemType.ParenthesisEnd, "parenthesis end");
            }
            else
                Parser.ParseValue(lexer, LexemType.Symbol, "option name");

            while (lexer.Current.Type == LexemType.Dot)
            {
                lexer.Next();

                Parser.ParseValue(lexer, LexemType.Symbol, "option name");
            }

            Parser.ParseValue(lexer, LexemType.Equal, "equal sign");
            Parser.ParseConstant(lexer);
        }

        private static void ParseOptions(Lexer lexer)
        {
            for (bool first = true; first || lexer.Current.Type == LexemType.Comma; first = false)
            {
                if (!first)
                    lexer.Next();

                Parser.ParseOption(lexer);
            }
        }

        private static ProtoReference ParseType(Lexer lexer, IEnumerable<string> parentNames)
        {
            bool local;

            if (lexer.Current.Type == LexemType.Dot)
            {
                lexer.Next();

                local = false;
            }
            else
                local = true;

            var ident = Parser.ParseFullIdent(lexer);

            if (Enum.TryParse(ident.ToLowerInvariant(), true, out ProtoType type))
                return new ProtoReference(type);

            if (local)
                return new ProtoReference(parentNames.Concat(new [] { ident }));

            return new ProtoReference(new [] { ident });
        }

        private static string ParseValue(Lexer lexer, LexemType type, string expected)
        {
            string value = lexer.Current.Value;

            if (lexer.Current.Type != type)
                throw new ParserException(lexer.Position, "expected {0}, found '{1}'", expected, value);

            lexer.Next();

            return value;
        }
    }
}
