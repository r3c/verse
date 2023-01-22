using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON;

internal class WriterState : IDisposable
{
    private const char AsciiUpperBound = (char) 128;

    private bool isEmpty;

    private string nextKey;

    private bool needComma;

    private readonly bool omitNull;

    private readonly StreamWriter writer;

    private static readonly string[] Ascii = new string[AsciiUpperBound];

    private static readonly char[] Hexa =
        {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

    public WriterState(Stream stream, Encoding encoding, bool omitNull)
    {
        isEmpty = true;
        nextKey = null;
        needComma = false;
        this.omitNull = omitNull;
        writer = new StreamWriter(stream, encoding, 1024, true);
    }

    static WriterState()
    {
        for (var i = 0; i < 32; ++i)
            Ascii[i] = "\\u00" + Hexa[(i >> 4) & 0xF] + Hexa[(i >> 0) & 0xF];

        for (var i = 32; i < AsciiUpperBound; ++i)
            Ascii[i] = new string((char) i, 1);

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
        writer.Write('[');

        isEmpty = false;
        needComma = false;
    }

    public void ArrayEnd()
    {
        writer.Write(']');

        isEmpty = false;
        needComma = true;
    }

    public void Dispose()
    {
        writer.Dispose();
    }

    public void Flush()
    {
        if (isEmpty)
            AppendNull();

        isEmpty = true;
        nextKey = null;
        needComma = false;
    }

    public void Key(string key)
    {
        nextKey = key;
    }

    public void ObjectBegin()
    {
        AppendPrefix();
        writer.Write('{');

        isEmpty = false;
        needComma = false;
    }

    public void ObjectEnd()
    {
        writer.Write('}');

        isEmpty = false;
        needComma = true;
    }

    public void Value(JSONValue value)
    {
        switch (value.Type)
        {
            case JSONType.Boolean:
                AppendPrefix();
                writer.Write(value.Boolean ? "true" : "false");

                break;

            case JSONType.Number:
                AppendPrefix();
                writer.Write(value.Number.ToString(CultureInfo.InvariantCulture));

                break;

            case JSONType.String:
                AppendPrefix();
                WriteString(writer, value.String);

                break;

            default:
                if (omitNull)
                {
                    nextKey = null;

                    return;
                }

                AppendPrefix();
                AppendNull();

                break;
        }

        isEmpty = false;
        needComma = true;
    }

    private void AppendNull()
    {
        writer.Write("null");
    }

    private void AppendPrefix()
    {
        if (needComma)
            writer.Write(',');

        if (nextKey == null)
            return;

        WriteString(writer, nextKey);

        writer.Write(':');
        nextKey = null;
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
                writer.Write(Hexa[(c >> 12) & 0xF]);
                writer.Write(Hexa[(c >> 8) & 0xF]);
                writer.Write(Hexa[(c >> 4) & 0xF]);
                writer.Write(Hexa[(c >> 0) & 0xF]);
            }
        }

        writer.Write('"');
    }
}