using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.Protobuf.Definition
{
    internal class Lexer
    {
        public Lexem Current => current;

        public int Position => position;

        private Lexem current;

        private int pending;

        private int position;

        private readonly TextReader reader;

        public Lexer(TextReader reader)
        {
            pending = reader.Read();
            position = 1;
            this.reader = reader;

            Next();
        }

        public void Next()
        {
            StringBuilder builder;
            LexemType type;

            // Skip whitespaces
            while (pending >= 0 && pending <= ' ')
                Read();

            // Match current character
            if (pending < 0)
                type = LexemType.End;
            else if (pending == '{')
                type = LexemType.BraceBegin;
            else if (pending == '}')
                type = LexemType.BraceEnd;
            else if (pending == '[')
                type = LexemType.BracketBegin;
            else if (pending == ']')
                type = LexemType.BracketEnd;
            else if (pending == ',')
                type = LexemType.Comma;
            else if (pending == '=')
                type = LexemType.Equal;
            else if (pending == '.')
                type = LexemType.Dot;
            else if (pending == '>')
                type = LexemType.GreaterThan;
            else if (pending == '<')
                type = LexemType.LowerThan;
            else if (pending == '-')
                type = LexemType.Minus;
            else if (pending == ';')
                type = LexemType.SemiColon;
            else if (pending == '(')
                type = LexemType.ParenthesisBegin;
            else if (pending == '+')
                type = LexemType.Plus;
            else if (pending == ')')
                type = LexemType.ParenthesisEnd;
            else if (pending == '/')
            {
                Read();

                if (pending == '/')
                {
                    while (pending >= 0 && pending != '\n')
                        Read();

                    Next();

                    return;
                }

                type = LexemType.Unknown;
            }
            else if (pending >= '0' && pending <= '9')
            {
                builder = new StringBuilder();

                do
                {
                    builder.Append((char)pending);

                    Read();
                }
                while (pending >= '0' && pending <= '9');

                current = new Lexem(LexemType.Number, builder.ToString());

                return;
            }
            else if ((pending >= 'A' && pending <= 'Z') || (pending >= 'a' && pending <= 'z') || pending == '_')
            {
                builder = new StringBuilder();

                do
                {
                    builder.Append((char)pending);

                    Read();
                }
                while ((pending >= '0' && pending <= '9') || (pending >= 'A' && pending <= 'Z') || (pending >= 'a' && pending <= 'z') || pending == '_');

                current = new Lexem(LexemType.Symbol, builder.ToString());

                return;
            }
            else if (pending == '\'' || pending == '"')
            {
                var delimiter = pending;

                builder = new StringBuilder();

                while (true)
                {
                    pending = reader.Read();

                    ++position;

                    if (pending == delimiter)
                        break;

                    if (pending == '\\')
                    {
                        pending = reader.Read();

                        ++position;

                        if (pending == 'X' || pending == 'x')
                        {
                            var hex1 = reader.Read();
                            var hex2 = reader.Read();

                            position += 2;

                            if (((hex1 < '0' || hex1 > '9') && (hex1 < 'A' || hex1 > 'F') && (hex1 < 'a' || hex1 > 'f')) ||
                                ((hex2 < '0' || hex2 > '9') && (hex2 < 'A' || hex2 > 'F') && (hex2 < 'a' || hex2 > 'f')))
                            {
                                current = new Lexem(LexemType.Unknown, new string(new [] {(char)hex1, (char)hex2}));

                                return;
                            }

                            builder.Append((char)int.Parse(string.Empty, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
                        }
                        else if (pending == '0')
                        {
                            var oct1 = reader.Read();
                            var oct2 = reader.Read();
                            var oct3 = reader.Read();

                            position += 3;

                            if (oct1 < '0' || oct1 > '9' || oct2 < '0' || oct2 > '9' || oct3 < '0' || oct3 > '9')
                            {
                                current = new Lexem(LexemType.Unknown, new string(new [] {(char)oct1, (char)oct2, (char)oct3}));

                                return;
                            }

                            builder.Append((char)((oct1 - '0') * 8 * 8 + (oct2 - '0') * 8 + (oct3 - '0')));
                        }
                        else if (pending == 'a')
                            builder.Append('\a');
                        else if (pending == 'b')
                            builder.Append('\b');
                        else if (pending == 'f')
                            builder.Append('\f');
                        else if (pending == 'n')
                            builder.Append('\n');
                        else if (pending == 'r')
                            builder.Append('\r');
                        else if (pending == 't')
                            builder.Append('\t');
                        else if (pending == 'v')
                            builder.Append('\v');
                        else if (pending == '\\')
                            builder.Append('\\');
                        else if (pending == '\'')
                            builder.Append('\'');
                        else if (pending == '"')
                            builder.Append('"');
                        else
                        {
                            current = new Lexem(LexemType.Unknown, new string((char)pending, 1));

                            return;
                        }

                        Read();
                    }
                    else if (pending > 0 && pending != '\n' && pending != '\\')
                        builder.Append((char)pending);
                    else
                    {
                        current = new Lexem(LexemType.Unknown, new string((char)pending, 1));

                        return;
                    }
                }

                current = new Lexem(LexemType.String, builder.ToString());

                Read();

                return;
            }
            else
                type = LexemType.Unknown;

            current = new Lexem(type, pending > 0 ? new string((char)pending, 1) : string.Empty);

            Read();
        }

        private void Read()
        {
            pending = reader.Read();

            ++position;
        }
    }
}
