using System;
using System.IO;
using System.Text;

namespace Verse.Schemas.Json;

internal class ReaderState : IDisposable
{
    public int Current;

    private readonly ErrorEvent _error;

    private int _position;

    private readonly StreamReader _reader;

    public ReaderState(Stream stream, Encoding encoding, ErrorEvent error)
    {
        _error = error;
        _position = 0;
        _reader = new StreamReader(stream, encoding, false, 1024, true);

        Read();
    }

    public void Dispose()
    {
        _reader.Dispose();
    }

    public void Error(string message)
    {
        _error(_position, message);
    }

    public bool PullCharacter(out char character)
    {
        var previous = Current;

        Read();

        if (previous < 0)
        {
            character = '\0';

            return false;
        }

        if (previous != '\\')
        {
            character = (char)previous;

            return true;
        }

        previous = Current;

        Read();

        switch (previous)
        {
            case -1:
                character = '\0';

                return false;

            case '"':
                character = '"';

                return true;

            case '\\':
                character = '\\';

                return true;

            case 'b':
                character = '\b';

                return true;

            case 'f':
                character = '\f';

                return true;

            case 'n':
                character = '\n';

                return true;

            case 'r':
                character = '\r';

                return true;

            case 't':
                character = '\t';

                return true;

            case 'u':
                var value = 0;

                for (var i = 0; i < 4; ++i)
                {
                    previous = Current;

                    Read();

                    int nibble;

                    if (previous >= '0' && previous <= '9')
                        nibble = previous - '0';
                    else if (previous >= 'A' && previous <= 'F')
                        nibble = previous - 'A' + 10;
                    else if (previous >= 'a' && previous <= 'f')
                        nibble = previous - 'a' + 10;
                    else
                    {
                        Error("unknown character in unicode escape sequence");

                        character = '\0';

                        return false;
                    }

                    value = (value << 4) + nibble;
                }

                character = (char)value;

                return true;

            default:
                character = (char)previous;

                return true;
        }
    }

    public bool PullExpected(char expected)
    {
        if (Current != expected)
        {
            Error("expected '" + expected + "'");

            return false;
        }

        Read();

        return true;
    }

    public void PullIgnored()
    {
        while (Current >= 0 && Current <= ' ')
            Read();
    }

    public void Read()
    {
        Current = _reader.Read();

        ++_position;
    }
}