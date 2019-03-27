using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.Protobuf.Definition
{
    class Lexer
    {
        public Lexem Current => this.current;

        public int Position => this.position;

        private Lexem current;

        private int pending;

        private int position;

        private readonly TextReader reader;

        public Lexer(TextReader reader)
        {
            this.pending = reader.Read();
            this.position = 1;
            this.reader = reader;

            this.Next();
        }

        public void Next()
        {
            StringBuilder builder;
            LexemType type;

            // Skip whitespaces
            while (this.pending >= 0 && this.pending <= ' ')
                this.Read();

            // Match current character
            if (this.pending < 0)
                type = LexemType.End;
            else if (this.pending == '{')
                type = LexemType.BraceBegin;
            else if (this.pending == '}')
                type = LexemType.BraceEnd;
            else if (this.pending == '[')
                type = LexemType.BracketBegin;
            else if (this.pending == ']')
                type = LexemType.BracketEnd;
            else if (this.pending == ',')
                type = LexemType.Comma;
            else if (this.pending == '=')
                type = LexemType.Equal;
            else if (this.pending == '.')
                type = LexemType.Dot;
            else if (this.pending == '>')
                type = LexemType.GreaterThan;
            else if (this.pending == '<')
                type = LexemType.LowerThan;
            else if (this.pending == '-')
                type = LexemType.Minus;
            else if (this.pending == ';')
                type = LexemType.SemiColon;
            else if (this.pending == '(')
                type = LexemType.ParenthesisBegin;
            else if (this.pending == '+')
                type = LexemType.Plus;
            else if (this.pending == ')')
                type = LexemType.ParenthesisEnd;
            else if (this.pending == '/')
            {
                this.Read();

                if (this.pending == '/')
                {
                    while (this.pending >= 0 && this.pending != '\n')
                        this.Read();

                    this.Next();

                    return;
                }

                type = LexemType.Unknown;
            }
            else if (this.pending >= '0' && this.pending <= '9')
            {
                builder = new StringBuilder();

                do
                {
                    builder.Append((char)this.pending);

                    this.Read();
                }
                while (this.pending >= '0' && this.pending <= '9');

                this.current = new Lexem(LexemType.Number, builder.ToString());

                return;
            }
            else if ((this.pending >= 'A' && this.pending <= 'Z') || (this.pending >= 'a' && this.pending <= 'z') || this.pending == '_')
            {
                builder = new StringBuilder();

                do
                {
                    builder.Append((char)this.pending);

                    this.Read();
                }
                while ((this.pending >= '0' && this.pending <= '9') || (this.pending >= 'A' && this.pending <= 'Z') || (this.pending >= 'a' && this.pending <= 'z') || this.pending == '_');

                this.current = new Lexem(LexemType.Symbol, builder.ToString());

                return;
            }
            else if (this.pending == '\'' || this.pending == '"')
            {
                var delimiter = this.pending;

                builder = new StringBuilder();

                while (true)
                {
                    this.pending = this.reader.Read();

                    ++this.position;

                    if (this.pending == delimiter)
                        break;

                    if (this.pending == '\\')
                    {
                        this.pending = this.reader.Read();

                        ++this.position;

                        if (this.pending == 'X' || this.pending == 'x')
                        {
                            int hex1 = this.reader.Read();
                            int hex2 = this.reader.Read();

                            this.position += 2;

                            if (((hex1 < '0' || hex1 > '9') && (hex1 < 'A' || hex1 > 'F') && (hex1 < 'a' || hex1 > 'f')) ||
                                ((hex2 < '0' || hex2 > '9') && (hex2 < 'A' || hex2 > 'F') && (hex2 < 'a' || hex2 > 'f')))
                            {
                                this.current = new Lexem(LexemType.Unknown, new string(new [] {(char)hex1, (char)hex2}));

                                return;
                            }

                            builder.Append((char)int.Parse(string.Empty, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
                        }
                        else if (this.pending == '0')
                        {
                            int oct1 = this.reader.Read();
                            int oct2 = this.reader.Read();
                            int oct3 = this.reader.Read();

                            this.position += 3;

                            if (oct1 < '0' || oct1 > '9' || oct2 < '0' || oct2 > '9' || oct3 < '0' || oct3 > '9')
                            {
                                this.current = new Lexem(LexemType.Unknown, new string(new [] {(char)oct1, (char)oct2, (char)oct3}));

                                return;
                            }

                            builder.Append((char)((oct1 - '0') * 8 * 8 + (oct2 - '0') * 8 + (oct3 - '0')));
                        }
                        else if (this.pending == 'a')
                            builder.Append('\a');
                        else if (this.pending == 'b')
                            builder.Append('\b');
                        else if (this.pending == 'f')
                            builder.Append('\f');
                        else if (this.pending == 'n')
                            builder.Append('\n');
                        else if (this.pending == 'r')
                            builder.Append('\r');
                        else if (this.pending == 't')
                            builder.Append('\t');
                        else if (this.pending == 'v')
                            builder.Append('\v');
                        else if (this.pending == '\\')
                            builder.Append('\\');
                        else if (this.pending == '\'')
                            builder.Append('\'');
                        else if (this.pending == '"')
                            builder.Append('"');
                        else
                        {
                            this.current = new Lexem(LexemType.Unknown, new string((char)this.pending, 1));

                            return;
                        }

                        this.Read();
                    }
                    else if (this.pending > 0 && this.pending != '\n' && this.pending != '\\')
                        builder.Append((char)this.pending);
                    else
                    {
                        this.current = new Lexem(LexemType.Unknown, new string((char)this.pending, 1));

                        return;
                    }
                }

                this.current = new Lexem(LexemType.String, builder.ToString());

                this.Read();

                return;
            }
            else
                type = LexemType.Unknown;

            this.current = new Lexem(type, this.pending > 0 ? new string((char)this.pending, 1) : string.Empty);

            this.Read();
        }

        private void Read()
        {
            this.pending = this.reader.Read();

            ++this.position;
        }
    }
}
