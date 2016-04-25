using System;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON
{
    class ReaderState
    {
        #region Attributes / Public

        public int Current;

        public readonly ParserError OnError;

        public int Position;

        #endregion

        #region Attributes / Private

        private readonly StreamReader reader;

        #endregion

        #region Constructors

        public ReaderState(Stream stream, Encoding encoding, ParserError onError)
        {
            this.reader = new StreamReader(stream, encoding);

            this.OnError = onError;
            this.Position = 0;

            this.Read();
        }

        #endregion

        #region Methods

        public bool PullCharacter(out char character)
        {
            int nibble;
            int previous;
            int value;

            previous = this.Current;

            this.Read();

            if (previous < 0)
            {
                character = default (char);

                return false;
            }

            if (previous != (int)'\\')
            {
                character = (char)previous;

                return true;
            }

            previous = this.Current;

            this.Read();

            switch (previous)
            {
                case -1:
                    character = default (char);

                    return false;

                case (int)'"':
                    character = '"';

                    return true;

                case (int)'\\':
                    character = '\\';

                    return true;

                case (int)'b':
                    character = '\b';

                    return true;

                case (int)'f':
                    character = '\f';

                    return true;

                case (int)'n':
                    character = '\n';

                    return true;

                case (int)'r':
                    character = '\r';

                    return true;

                case (int)'t':
                    character = '\t';

                    return true;

                case (int)'u':
                    value = 0;

                    for (int i = 0; i < 4; ++i)
                    {
                        previous = this.Current;

                        this.Read();

                        if (previous >= (int)'0' && previous <= (int)'9')
                            nibble = previous - (int)'0';
                        else if (previous >= (int)'A' && previous <= (int)'F')
                            nibble = previous - (int)'A' + 10;
                        else if (previous >= (int)'a' && previous <= (int)'f')
                            nibble = previous - (int)'a' + 10;
                        else
                        {
                            this.OnError(this.Position, "unknown character in unicode escape sequence");

                            character = default (char);

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
            if (this.Current != (int)expected)
            {
                this.OnError(this.Position, "expected '" + expected + "'");

                return false;
            }

            this.Read();

            return true;
        }

        public void PullIgnored()
        {
            int current;

            while (true)
            {
                current = this.Current;

                if (current < 0 || current > (int)' ')
                    return;

                this.Read();
            }
        }

        public void Read()
        {
            this.Current = this.reader.Read();

            ++this.Position;
        }

        #endregion
    }
}