using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.Formats.Json;

namespace Verse.Schemas.Json;

internal class WriterState(Stream stream, Encoding encoding, bool omitNull) : IDisposable
{
    private const char AsciiUpperBound = (char)128;

    private bool _isEmpty = true;

    private string? _nextKey;

    private bool _needComma;

    private readonly StreamWriter _writer = new(stream, encoding, 1024, true);

    private static readonly string[] Ascii = new string[AsciiUpperBound];

    private static readonly char[] Hexa =
        ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];

    static WriterState()
    {
        for (var i = 0; i < 32; ++i)
            Ascii[i] = "\\u00" + Hexa[i >> 4 & 0xF] + Hexa[i >> 0 & 0xF];

        for (var i = 32; i < AsciiUpperBound; ++i)
            Ascii[i] = new string((char)i, 1);

        Ascii['\b'] = "\\b";
        Ascii['\f'] = "\\f";
        Ascii['\n'] = "\\n";
        Ascii['\r'] = "\\r";
        Ascii['\t'] = "\\t";
        Ascii['\\'] = "\\\\";
        Ascii['"'] = "\\\"";
    }

    public void ArrayBegin()
    {
        AppendPrefix();
        _writer.Write('[');

        _isEmpty = false;
        _needComma = false;
    }

    public void ArrayEnd()
    {
        _writer.Write(']');

        _isEmpty = false;
        _needComma = true;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    public bool Flush()
    {
        if (_isEmpty)
            AppendNull();

        _isEmpty = true;
        _nextKey = null;
        _needComma = false;

        return true;
    }

    public void Key(string key)
    {
        _nextKey = key;
    }

    public void ObjectBegin()
    {
        AppendPrefix();
        _writer.Write('{');

        _isEmpty = false;
        _needComma = false;
    }

    public void ObjectEnd()
    {
        _writer.Write('}');

        _isEmpty = false;
        _needComma = true;
    }

    public bool Value(JsonValue value)
    {
        switch (value.Type)
        {
            case JsonType.Boolean:
                AppendPrefix();
                _writer.Write(value.Boolean ? "true" : "false");

                break;

            case JsonType.Number:
                AppendPrefix();
                _writer.Write(value.Number.ToString(CultureInfo.InvariantCulture));

                break;

            case JsonType.String:
                AppendPrefix();
                WriteString(_writer, value.String);

                break;

            case JsonType.Undefined:
                if (omitNull)
                {
                    _nextKey = null;

                    return true;
                }

                AppendPrefix();
                AppendNull();

                break;

            default:
                return false;
        }

        _isEmpty = false;
        _needComma = true;

        return true;
    }

    private void AppendNull()
    {
        _writer.Write("null");
    }

    private void AppendPrefix()
    {
        if (_needComma)
            _writer.Write(',');

        if (_nextKey == null)
            return;

        WriteString(_writer, _nextKey);

        _writer.Write(':');
        _nextKey = null;
    }

    private static void WriteString(TextWriter writer, string value)
    {
        writer.Write('"');

        foreach (var c in value)
        {
            if (c < AsciiUpperBound)
                writer.Write(Ascii[c]);
            else
            {
                writer.Write("\\u");
                writer.Write(Hexa[c >> 12 & 0xF]);
                writer.Write(Hexa[c >> 8 & 0xF]);
                writer.Write(Hexa[c >> 4 & 0xF]);
                writer.Write(Hexa[c >> 0 & 0xF]);
            }
        }

        writer.Write('"');
    }
}