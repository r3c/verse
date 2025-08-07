using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Verse.Formats.Protobuf;

namespace Verse.Schemas.Protobuf.Definition;

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
internal static class Parser
{
    public static ProtoEntity Parse(TextReader proto)
    {
        var lexer = new Lexer(proto);
        var root = new ProtoEntity(ProtoContainer.Root, string.Empty);

        for (var declarationNumber = 0; lexer.Current.Type != LexemType.End; ++declarationNumber)
        {
            var current = lexer.Current;

            if (current == Lexem.Edition)
            {
                if (declarationNumber > 0)
                    throw new InvalidOperationException($"keyword \"{current.Value}\" must be in first position");

                lexer.Next();

                ParseType(lexer, LexemType.Equal, "equal sign");

                var edition = ParseType(lexer, LexemType.String, "edition number");

                if (!int.TryParse(edition, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    throw new InvalidOperationException($"edition \"{edition}\" is not a valid number");

                ParseType(lexer, LexemType.SemiColon, "semicolon");
            }
            else if (current == Lexem.Option)
            {
                lexer.Next();

                ParseOption(lexer);
                ParseType(lexer, LexemType.SemiColon, "semicolon");
            }
            else if (current == Lexem.Syntax)
            {
                if (declarationNumber > 0)
                    throw new InvalidOperationException($"keyword \"{current.Value}\" must be in first position");

                lexer.Next();

                ParseType(lexer, LexemType.Equal, "equal sign");

                var syntax = ParseType(lexer, LexemType.String, "edition number");

                if (syntax != "proto2" && syntax != "proto3")
                    throw new InvalidOperationException($"syntax \"{syntax}\" is not a valid identifier");

                ParseType(lexer, LexemType.SemiColon, "semicolon");
            }
            else
            {
                var entity = ParseEntity(lexer);

                root.Entities.Add(entity);
            }
        }

        return root;
    }

    private static ProtobufValue ParseConstant(Lexer lexer)
    {
        switch (lexer.Current.Type)
        {
            case LexemType.Minus:
                lexer.Next();

                if (lexer.Current.Type == LexemType.Number)
                    return ParseNumber(lexer, -1);

                break;

            case LexemType.Number:
                return ParseNumber(lexer, 1);

            case LexemType.Plus:
                lexer.Next();

                if (lexer.Current.Type == LexemType.Number)
                    return ParseNumber(lexer, 1);

                break;

            case LexemType.String:
                return new ProtobufValue(ParseType(lexer, LexemType.String, string.Empty));

            case LexemType.Symbol:
                ProtobufValue value;

                if (lexer.Current.Value == "false")
                    value = default; // FIXME: new ProtobufValue(false);
                else if (lexer.Current.Value == "true")
                    value = default; // FIXME: new ProtobufValue(true);
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
                return ParseEnum(lexer);
            else if (value == "message")
                return ParseMessage(lexer, new string[0]);
        }

        throw new ParserException(lexer.Position, "expected enum or message definition");
    }

    private static ProtoEntity ParseEnum(Lexer lexer)
    {
        var entity = new ProtoEntity(ProtoContainer.Enum, ParseType(lexer, LexemType.Symbol, "enum name"));

        ParseType(lexer, LexemType.BraceBegin, "opening brace");

        while (lexer.Current.Type != LexemType.BraceEnd)
        {
            var label = ParseType(lexer, LexemType.Symbol, "enum label");

            if (label == "option")
                ParseOption(lexer);
            else
            {
                ParseType(lexer, LexemType.Equal, "equal sign");

                int value;
                if (!int.TryParse(ParseType(lexer, LexemType.Number, "enum value"), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out value))
                    throw new ParserException(lexer.Position, "invalid enum value");

                if (lexer.Current.Type == LexemType.BracketBegin)
                {
                    lexer.Next();

                    ParseOptions(lexer);
                    ParseType(lexer, LexemType.BracketEnd, "closing bracket");
                }

                entity.Labels.Add(new ProtoLabel(value, label));
            }

            ParseType(lexer, LexemType.SemiColon, "semicolon");
        }

        lexer.Next();

        if (entity.Labels.Count < 1 || entity.Labels[0].Value != 0)
            throw new ParserException(lexer.Position, "first enum value must be zero");

        return entity;
    }

    private static ProtoField ParseField(Lexer lexer, ProtoReference reference, ProtoOccurrence occurrence)
    {
        string name = ParseType(lexer, LexemType.Symbol, "field name");

        ParseType(lexer, LexemType.Equal, "equal sign");

        if (!int.TryParse(ParseType(lexer, LexemType.Number, "field number"), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var number))
            throw new ParserException(lexer.Position, "invalid field number");

        if (lexer.Current.Type == LexemType.BracketBegin)
        {
            lexer.Next();

            ParseOptions(lexer);
            ParseType(lexer, LexemType.BracketEnd, "closing bracket");
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

            name += ParseType(lexer, LexemType.Symbol, "identifier");

            if (lexer.Current.Type != LexemType.Dot)
                break;

            lexer.Next();
        }
        while (true);

        return name;
    }

    private static (ProtoEntity, ProtoField) ParseMapField(Lexer lexer, IReadOnlyList<string> parentNames)
    {
        var entity = new ProtoEntity(ProtoContainer.Message, Guid.NewGuid().ToString("N"));

        ParseType(lexer, LexemType.LowerThan, "lower than sign");

        entity.Fields.Add(new ProtoField(1, ParseReference(lexer, parentNames), "key", ProtoOccurrence.Required));

        ParseType(lexer, LexemType.Comma, "comma");

        entity.Fields.Add(new ProtoField(2, ParseReference(lexer, parentNames), "value", ProtoOccurrence.Required));

        ParseType(lexer, LexemType.GreaterThan, "greater than sign");

        var reference = new ProtoReference(parentNames.Append(entity.Name));
        var field = ParseField(lexer, reference, ProtoOccurrence.Required);

        return (entity, field);
    }

    private static ProtoEntity ParseMessage(Lexer lexer, IReadOnlyList<string> parentNames)
    {
        var name = ParseType(lexer, LexemType.Symbol, "message name");

        var currentNames = parentNames.Append(name).ToList();
        var entity = new ProtoEntity(ProtoContainer.Message, name);

        ParseType(lexer, LexemType.BraceBegin, "opening brace");

        while (lexer.Current.Type != LexemType.BraceEnd)
        {
            if (lexer.Current == Lexem.Enum)
            {
                lexer.Next();

                entity.Entities.Add(ParseEnum(lexer));
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

                var (mapEntity, mapField) = ParseMapField(lexer, currentNames);

                entity.Entities.Add(mapEntity);
                entity.Fields.Add(mapField);

                ParseType(lexer, LexemType.SemiColon, "semicolon");
            }
            else if (lexer.Current == Lexem.Message)
            {
                lexer.Next();

                entity.Entities.Add(ParseMessage(lexer, currentNames));
            }
            else if (lexer.Current == Lexem.OneOf)
            {
                lexer.Next();

                entity.Entities.Add(ParseOneOf(lexer, currentNames));
            }
            else if (lexer.Current == Lexem.Option)
            {
                lexer.Next();

                ParseOption(lexer);
                ParseType(lexer, LexemType.SemiColon, "semicolon");
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

                entity.Fields.Add(ParseField(lexer, ParseReference(lexer, parentNames), occurrence));

                ParseType(lexer, LexemType.SemiColon, "semicolon");
            }
        }

        lexer.Next();

        return entity;
    }

    private static ProtobufValue ParseNumber(Lexer lexer, int sign)
    {
        var number = lexer.Current.Value;

        lexer.Next();

        // FIXME: oct and hex literals not handled
        if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInteger))
            return new ProtobufValue(sign * asInteger);
        else if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var asDecimal))
            return default; // FIXME: new ProtobufValue(sign * asDecimal);
        else
            throw new ParserException(lexer.Position, "invalid literal number");
    }

    private static ProtoEntity ParseOneOf(Lexer lexer, IReadOnlyList<string> parentNames)
    {
        var entity = new ProtoEntity(ProtoContainer.OneOf, ParseType(lexer, LexemType.Symbol, "oneof name"));

        ParseType(lexer, LexemType.BraceBegin, "opening brace");

        while (lexer.Current.Type != LexemType.BraceEnd)
        {
            entity.Fields.Add(ParseField(lexer, ParseReference(lexer, parentNames), ProtoOccurrence.Required));

            ParseType(lexer, LexemType.SemiColon, "semicolon");
        }

        lexer.Next();

        return entity;
    }

    private static void ParseOption(Lexer lexer)
    {
        if (lexer.Current.Type == LexemType.ParenthesisBegin)
        {
            lexer.Next();

            ParseFullIdent(lexer);
            ParseType(lexer, LexemType.ParenthesisEnd, "parenthesis end");
        }
        else
            ParseType(lexer, LexemType.Symbol, "option name");

        while (lexer.Current.Type == LexemType.Dot)
        {
            lexer.Next();

            ParseType(lexer, LexemType.Symbol, "option name");
        }

        ParseType(lexer, LexemType.Equal, "equal sign");
        ParseConstant(lexer);
    }

    private static void ParseOptions(Lexer lexer)
    {
        for (bool first = true; first || lexer.Current.Type == LexemType.Comma; first = false)
        {
            if (!first)
                lexer.Next();

            ParseOption(lexer);
        }
    }

    private static ProtoReference ParseReference(Lexer lexer, IEnumerable<string> parentNames)
    {
        bool local;

        if (lexer.Current.Type == LexemType.Dot)
        {
            lexer.Next();

            local = false;
        }
        else
            local = true;

        var ident = ParseFullIdent(lexer);

        if (Enum.TryParse(ident, true, out ProtoType type) && type != ProtoType.Custom && type != ProtoType.Undefined)
            return new ProtoReference(type);

        return local ? new ProtoReference(parentNames.Append(ident)) : new ProtoReference([ident]);
    }

    private static string ParseType(Lexer lexer, LexemType type, string description)
    {
        var value = lexer.Current.Value;

        if (lexer.Current.Type != type)
            throw new ParserException(lexer.Position, "expected {0}, found '{1}'", description, value);

        lexer.Next();

        return value;
    }
}