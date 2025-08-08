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
        var language = new Language(LanguageVersion.Proto2);
        var lexer = new Lexer(proto);
        var root = new ProtoEntity(ProtoContainer.Root, string.Empty);

        for (var declarationNumber = 0; lexer.Current.Type != LexemType.End; ++declarationNumber)
        {
            if (TryParseLexem(lexer, Lexem.Edition))
            {
                if (declarationNumber > 0)
                    throw new InvalidOperationException($"keyword \"{Lexem.Edition.Value}\" must be in first position");

                ParseType(lexer, LexemType.Equal, "equal sign");

                var edition = ParseType(lexer, LexemType.String, "edition number");

                if (!int.TryParse(edition, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                    throw new InvalidOperationException($"edition \"{edition}\" is not a valid number");

                ParseType(lexer, LexemType.SemiColon, "semicolon");

                var featureVersion = number switch
                {
                    _ => LanguageVersion.Edition2023
                };

                language = new Language(featureVersion);
            }
            else if (TryParseLexem(lexer, Lexem.Option))
                ParseOption(lexer);
            else if (TryParseLexem(lexer, Lexem.Syntax))
            {
                if (declarationNumber > 0)
                    throw new InvalidOperationException($"keyword \"{Lexem.Syntax.Value}\" must be in first position");

                ParseType(lexer, LexemType.Equal, "equal sign");

                var syntax = ParseType(lexer, LexemType.String, "syntax identifier");

                ParseType(lexer, LexemType.SemiColon, "semicolon");

                var featureVersion = syntax switch
                {
                    "proto2" => LanguageVersion.Proto2,
                    "proto3" => LanguageVersion.Proto3,
                    _ => throw new InvalidOperationException($"syntax \"{syntax}\" is not a valid identifier")
                };

                language = new Language(featureVersion);
            }
            else
            {
                var entity = ParseEntity(lexer, language);

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
                var stringValue = ParseType(lexer, LexemType.String, string.Empty);

                return new ProtobufValue(stringValue);

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

    private static ProtoEntity ParseEntity(Lexer lexer, Language language)
    {
        if (TryParseLexem(lexer, Lexem.Enum))
            return ParseEnum(lexer, language);

        if (TryParseLexem(lexer, Lexem.Message))
            return ParseMessage(lexer, language, []);

        throw new ParserException(lexer.Position, "expected enum or message definition");
    }

    private static ProtoEntity ParseEnum(Lexer lexer, Language language)
    {
        var entity = new ProtoEntity(ProtoContainer.Enum, ParseType(lexer, LexemType.Symbol, "enum name"));

        ParseType(lexer, LexemType.BraceBegin, "opening brace");

        while (lexer.Current.Type != LexemType.BraceEnd)
        {
            var label = ParseType(lexer, LexemType.Symbol, "enum label");

            if (label == "option")
            {
                ParseOption(lexer);

                continue;
            }

            ParseType(lexer, LexemType.Equal, "equal sign");

            var enumValue = ParseType(lexer, LexemType.Number, "enum value");

            if (!int.TryParse(enumValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                throw new ParserException(lexer.Position, "invalid enum value");

            if (TryParseType(lexer, LexemType.BracketBegin))
            {
                ParseOptions(lexer);
                ParseType(lexer, LexemType.BracketEnd, "closing bracket");
            }

            entity.Labels.Add(new ProtoLabel(value, label));

            ParseType(lexer, LexemType.SemiColon, "semicolon");
        }

        lexer.Next();

        if (entity.Labels.Count < 1 || entity.Labels[0].Value != 0)
            throw new ParserException(lexer.Position, "first enum value must be zero");

        return entity;
    }

    private static ProtoField ParseField(Lexer lexer, ProtoReference reference, ProtoPresence presence)
    {
        var name = ParseType(lexer, LexemType.Symbol, "field name");

        ParseType(lexer, LexemType.Equal, "equal sign");

        var fieldNumber = ParseType(lexer, LexemType.Number, "field number");

        if (!int.TryParse(fieldNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            throw new ParserException(lexer.Position, "invalid field number");

        if (TryParseType(lexer, LexemType.BracketBegin))
        {
            ParseOptions(lexer);
            ParseType(lexer, LexemType.BracketEnd, "closing bracket");
        }

        return new ProtoField(number, reference, name, presence);
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

        entity.Fields.Add(new ProtoField(1, ParseReference(lexer, parentNames), "key", ProtoPresence.Required));

        ParseType(lexer, LexemType.Comma, "comma");

        entity.Fields.Add(new ProtoField(2, ParseReference(lexer, parentNames), "value", ProtoPresence.Required));

        ParseType(lexer, LexemType.GreaterThan, "greater than sign");

        var reference = new ProtoReference(parentNames.Append(entity.Name));
        var field = ParseField(lexer, reference, ProtoPresence.Required);

        return (entity, field);
    }

    private static ProtoEntity ParseMessage(Lexer lexer, Language language, IReadOnlyList<string> parentNames)
    {
        var name = ParseType(lexer, LexemType.Symbol, "message name");

        var currentNames = parentNames.Append(name).ToList();
        var entity = new ProtoEntity(ProtoContainer.Message, name);

        ParseType(lexer, LexemType.BraceBegin, "opening brace");

        while (lexer.Current.Type != LexemType.BraceEnd)
        {
            if (TryParseLexem(lexer, Lexem.Enum))
                entity.Entities.Add(ParseEnum(lexer, language));
            else if (TryParseLexem(lexer, Lexem.Extend))
                throw new InvalidOperationException("extensions are not supported yet");
            else if (TryParseLexem(lexer, Lexem.Extensions))
                throw new InvalidOperationException("extensions are not supported yet");
            else if (TryParseLexem(lexer, Lexem.Group))
                throw new InvalidOperationException("groups are not supported yet");
            else if (TryParseLexem(lexer, Lexem.Map))
            {
                var (mapEntity, mapField) = ParseMapField(lexer, currentNames);

                entity.Entities.Add(mapEntity);
                entity.Fields.Add(mapField);

                ParseType(lexer, LexemType.SemiColon, "semicolon");
            }
            else if (TryParseLexem(lexer, Lexem.Message))
                entity.Entities.Add(ParseMessage(lexer, language, currentNames));
            else if (TryParseLexem(lexer, Lexem.OneOf))
                entity.Entities.Add(ParseOneOf(lexer, currentNames));
            else if (TryParseLexem(lexer, Lexem.Option))
                ParseOption(lexer);
            else if (TryParseLexem(lexer, Lexem.Reserved))
                ParseReserved(lexer);
            else
            {
                var presence = ParsePresence(lexer, language);
                var reference = ParseReference(lexer, parentNames);
                var field = ParseField(lexer, reference, presence);

                entity.Fields.Add(field);

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
            var reference = ParseReference(lexer, parentNames);
            var field = ParseField(lexer, reference, ProtoPresence.Required);

            entity.Fields.Add(field);

            ParseType(lexer, LexemType.SemiColon, "semicolon");
        }

        lexer.Next();

        return entity;
    }

    private static void ParseOption(Lexer lexer)
    {
        ParseOptionKeyValue(lexer);
        ParseType(lexer, LexemType.SemiColon, "semicolon");
    }

    private static void ParseOptionKeyValue(Lexer lexer)
    {
        if (TryParseType(lexer, LexemType.ParenthesisBegin))
        {
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
        for (var first = true; first || lexer.Current.Type == LexemType.Comma; first = false)
        {
            if (!first)
                lexer.Next();

            ParseOptionKeyValue(lexer);
        }
    }

    private static ProtoPresence ParsePresence(Lexer lexer, Language language)
    {
        if (TryParseLexem(lexer, Lexem.Optional))
            return ProtoPresence.Optional;

        if (TryParseLexem(lexer, Lexem.Repeated))
            return ProtoPresence.Repeated;

        if (TryParseLexem(lexer, Lexem.Required))
            return ProtoPresence.Required;

        return ProtoPresence.Optional;
    }

    private static ProtoReference ParseReference(Lexer lexer, IEnumerable<string> parentNames)
    {
        var local = !TryParseType(lexer, LexemType.Dot);
        var ident = ParseFullIdent(lexer);

        if (Enum.TryParse(ident, true, out ProtoType type) && type != ProtoType.Custom && type != ProtoType.Undefined)
            return new ProtoReference(type);

        return local ? new ProtoReference(parentNames.Append(ident)) : new ProtoReference([ident]);
    }

    private static void ParseReserved(Lexer lexer)
    {
        while (TryParseType(lexer, LexemType.Number))
        {
            if (!TryParseType(lexer, LexemType.Comma) && !TryParseLexem(lexer, Lexem.To))
                break;
        }

        ParseType(lexer, LexemType.SemiColon, "semicolon");
    }

    private static string ParseType(Lexer lexer, LexemType type, string description)
    {
        var value = lexer.Current.Value;

        if (lexer.Current.Type != type)
            throw new ParserException(lexer.Position, "expected {0}, found '{1}'", description, value);

        lexer.Next();

        return value;
    }

    private static bool TryParseLexem(Lexer lexer, Lexem lexem)
    {
        if (lexer.Current != lexem)
            return false;

        lexer.Next();

        return true;
    }

    private static bool TryParseType(Lexer lexer, LexemType type)
    {
        if (lexer.Current.Type != type)
            return false;

        lexer.Next();

        return true;
    }
}