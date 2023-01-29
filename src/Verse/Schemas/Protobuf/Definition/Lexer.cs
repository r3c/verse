using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.Protobuf.Definition;

internal class Lexer
{
    public Lexem Current => _current;

    public int Position => _position;

    private Lexem _current;

    private int _pending;

    private int _position;

    private readonly TextReader _reader;

    public Lexer(TextReader reader)
    {
        _pending = reader.Read();
        _position = 1;
        _reader = reader;

        Next();
    }

    public void Next()
    {
        StringBuilder builder;
        LexemType type;

        // Skip whitespaces
        while (_pending >= 0 && _pending <= ' ')
            Read();

        // Match current character
        if (_pending < 0)
            type = LexemType.End;
        else if (_pending == '{')
            type = LexemType.BraceBegin;
        else if (_pending == '}')
            type = LexemType.BraceEnd;
        else if (_pending == '[')
            type = LexemType.BracketBegin;
        else if (_pending == ']')
            type = LexemType.BracketEnd;
        else if (_pending == ',')
            type = LexemType.Comma;
        else if (_pending == '=')
            type = LexemType.Equal;
        else if (_pending == '.')
            type = LexemType.Dot;
        else if (_pending == '>')
            type = LexemType.GreaterThan;
        else if (_pending == '<')
            type = LexemType.LowerThan;
        else if (_pending == '-')
            type = LexemType.Minus;
        else if (_pending == ';')
            type = LexemType.SemiColon;
        else if (_pending == '(')
            type = LexemType.ParenthesisBegin;
        else if (_pending == '+')
            type = LexemType.Plus;
        else if (_pending == ')')
            type = LexemType.ParenthesisEnd;
        else if (_pending == '/')
        {
            Read();

            if (_pending == '/')
            {
                while (_pending >= 0 && _pending != '\n')
                    Read();

                Next();

                return;
            }

            type = LexemType.Unknown;
        }
        else if (_pending >= '0' && _pending <= '9')
        {
            builder = new StringBuilder();

            do
            {
                builder.Append((char)_pending);

                Read();
            }
            while (_pending >= '0' && _pending <= '9');

            _current = new Lexem(LexemType.Number, builder.ToString());

            return;
        }
        else if ((_pending >= 'A' && _pending <= 'Z') || (_pending >= 'a' && _pending <= 'z') || _pending == '_')
        {
            builder = new StringBuilder();

            do
            {
                builder.Append((char)_pending);

                Read();
            }
            while ((_pending >= '0' && _pending <= '9') || (_pending >= 'A' && _pending <= 'Z') || (_pending >= 'a' && _pending <= 'z') || _pending == '_');

            _current = new Lexem(LexemType.Symbol, builder.ToString());

            return;
        }
        else if (_pending == '\'' || _pending == '"')
        {
            var delimiter = _pending;

            builder = new StringBuilder();

            while (true)
            {
                _pending = _reader.Read();

                ++_position;

                if (_pending == delimiter)
                    break;

                if (_pending == '\\')
                {
                    _pending = _reader.Read();

                    ++_position;

                    if (_pending == 'X' || _pending == 'x')
                    {
                        var hex1 = _reader.Read();
                        var hex2 = _reader.Read();

                        _position += 2;

                        if (((hex1 < '0' || hex1 > '9') && (hex1 < 'A' || hex1 > 'F') && (hex1 < 'a' || hex1 > 'f')) ||
                            ((hex2 < '0' || hex2 > '9') && (hex2 < 'A' || hex2 > 'F') && (hex2 < 'a' || hex2 > 'f')))
                        {
                            _current = new Lexem(LexemType.Unknown, new string(new[] { (char)hex1, (char)hex2 }));

                            return;
                        }

                        builder.Append((char)int.Parse(string.Empty, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
                    }
                    else if (_pending == '0')
                    {
                        var oct1 = _reader.Read();
                        var oct2 = _reader.Read();
                        var oct3 = _reader.Read();

                        _position += 3;

                        if (oct1 < '0' || oct1 > '9' || oct2 < '0' || oct2 > '9' || oct3 < '0' || oct3 > '9')
                        {
                            _current = new Lexem(LexemType.Unknown, new string(new[] { (char)oct1, (char)oct2, (char)oct3 }));

                            return;
                        }

                        builder.Append((char)((oct1 - '0') * 8 * 8 + (oct2 - '0') * 8 + (oct3 - '0')));
                    }
                    else if (_pending == 'a')
                        builder.Append('\a');
                    else if (_pending == 'b')
                        builder.Append('\b');
                    else if (_pending == 'f')
                        builder.Append('\f');
                    else if (_pending == 'n')
                        builder.Append('\n');
                    else if (_pending == 'r')
                        builder.Append('\r');
                    else if (_pending == 't')
                        builder.Append('\t');
                    else if (_pending == 'v')
                        builder.Append('\v');
                    else if (_pending == '\\')
                        builder.Append('\\');
                    else if (_pending == '\'')
                        builder.Append('\'');
                    else if (_pending == '"')
                        builder.Append('"');
                    else
                    {
                        _current = new Lexem(LexemType.Unknown, new string((char)_pending, 1));

                        return;
                    }

                    Read();
                }
                else if (_pending > 0 && _pending != '\n' && _pending != '\\')
                    builder.Append((char)_pending);
                else
                {
                    _current = new Lexem(LexemType.Unknown, new string((char)_pending, 1));

                    return;
                }
            }

            _current = new Lexem(LexemType.String, builder.ToString());

            Read();

            return;
        }
        else
            type = LexemType.Unknown;

        _current = new Lexem(type, _pending > 0 ? new string((char)_pending, 1) : string.Empty);

        Read();
    }

    private void Read()
    {
        _pending = _reader.Read();

        ++_position;
    }
}