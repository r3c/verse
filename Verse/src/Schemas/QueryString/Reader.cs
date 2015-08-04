using System;
using System.IO;
using System.Text;
using Verse.ParserDescriptors.Flat;

namespace Verse.Schemas.QueryString
{
    class Reader : IReader<ReaderContext, string>
    {
        #region Events

        public event ParserError Error;

        #endregion

        #region Attributes

        private readonly Encoding encoding;

        #endregion

        #region Constructors

        public Reader(Encoding encoding)
        {
            this.encoding = encoding;
        }

        #endregion

        #region Methods / Public

        public IBrowser<T> ReadArray<T>(Func<T> constructor, Container<T, ReaderContext, string> container, ReaderContext context)
        {
            throw new NotImplementedException("QueryString schema doesn't support arrays");
        }

        public bool ReadValue<T>(ref T target, Container<T, ReaderContext, string> container, ReaderContext context)
        {
            string value;

            if (!this.ReadFieldValue(context, out value))
                return false;

            if (container.value != null)
                container.value(ref target, value);

            return true;
        }

        public bool Read<T>(ref T target, Container<T, ReaderContext, string> container, ReaderContext context)
        {
            while (context.Current != -1)
            {
                bool isKeyEmpty;
                INode<T, ReaderContext, string> node;

                node = container.fields;
                isKeyEmpty = true;

                if (Reader.IsUnreservedCharacters(context.Current))
                {
                    node = node.Follow((char)context.Current);

                    isKeyEmpty = false;

                    context.Pull();
                }

                while (Reader.IsUnreservedCharacters(context.Current))
                {
                    node = node.Follow((char)context.Current);

                    context.Pull();
                }

                if (isKeyEmpty)
                {
                    this.OnError(context.Position, "empty field");

                    return false;
                }

                if (context.Current == '=')
                {
                    context.Pull();

                    if (!node.Enter(ref target, this, context))
                        return false;
                }

                if (context.Current == -1)
                    break;

                if (!Reader.IsSeparator(context.Current))
                {
                    this.OnError(context.Position, "unexpected character");

                    return false;
                }

                context.Pull();
            }

            return true;
        }

        public bool Start(Stream stream, out ReaderContext context)
        {
            context = new ReaderContext(stream, this.encoding);

            if (context.Current < 0)
            {
                this.OnError(context.Position, "empty input stream");

                return false;
            }

            if (context.Current != '?')
            {
                this.OnError(context.Position, "invalid character");

                return false;
            }

            context.Pull();

            return true;
        }

        public void Stop(ReaderContext context)
        {
        }

        #endregion

        #region Methods / Private

        private bool ReadFieldValue(ReaderContext context, out string value)
        {
            StringBuilder builder;

            builder = new StringBuilder(32);

            while (context.Current != -1)
            {
                int c;

                c = context.Current;

                if (Reader.IsUnreservedCharacters(c))
                {
                    builder.Append((char)c);
                }
                else if (c == '+')
                {
                    builder.Append(' ');
                }
                else if (c == '%')
                {
                    int digit1;
                    int digit2;
                    int hexaValue;
                    char[] result;

                    if (!Reader.ConvertToHexadecimalDigit(context, out digit1) ||
                        !Reader.ConvertToHexadecimalDigit(context, out digit2))
                    {
                        value = string.Empty;

                        this.OnError(context.Position, "invalid hexadecimal character");

                        return false;
                    }

                    hexaValue = digit1 * 16 + digit2;

                    result = Encoding.ASCII.GetChars(new[] { (byte)hexaValue });

                    if (result.Length != 1)
                    {
                        value = string.Empty;

                        this.OnError(context.Position, "invalid ascii character");

                        return false;
                    }

                    builder.Append(result[0]);
                }
                else
                    break;

                context.Pull();
            }

            value = builder.ToString();

            return true;
        }

        private void OnError(int position, string message)
        {
            ParserError error;

            error = this.Error;

            if (error != null)
                error(position, message);
        }

        private static bool ConvertToHexadecimalDigit(ReaderContext context, out int digit)
        {
            context.Pull();

            if (context.Current >= '0' && context.Current <= '9')
            {
                digit = context.Current - '0';
                return true;
            }
            else if (context.Current >= 'A' && context.Current <= 'F')
            {
                digit = context.Current - 'A' + 10;
                return true;
            }
            else if (context.Current >= 'a' && context.Current <= 'f')
            {
                digit = context.Current - 'a' + 10;
                return true;
            }

            digit = -1;

            return false;
        }

        static private bool IsSeparator(int ch)
        {
            return ch == '&' || ch == ';';
        }

        /// <summary>
        /// Check if character is unreserved.
        /// </summary>
        /// <param name="ch"></param>
        /// <remarks>Array is not supported yet (",")</remarks>
        /// <returns></returns>
        static private bool IsUnreservedCharacters(int ch)
        {
            return (ch >= 'A' && ch <= 'Z') ||
                   (ch >= 'a' && ch <= 'z') ||
                   (ch >= '0' && ch <= '9') ||
                   ch == '-' || ch == '_' || ch == '.' || ch == '!' ||
                   ch == '~' || ch == '*' || ch == '\'' || ch == '(' || ch == ')' ||
                   ch == ',' || ch == '"' || ch == '$' || ch == ':' || ch == '@' || ch == '/' || ch == '?';
        }

        #endregion
    }
}
