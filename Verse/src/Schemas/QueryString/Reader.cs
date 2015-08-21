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

        #region Attributes / Instance

        private readonly Encoding encoding;

        #endregion

        #region Attributes / Static

        private static readonly bool[] unreserved = new bool[128];

        #endregion

        #region Constructors

        public Reader(Encoding encoding)
        {
            this.encoding = encoding;
        }

        static Reader()
        {
            for (int c = '0'; c <= '9'; ++c)
                Reader.unreserved[c] = true;

            for (int c = 'A'; c <= 'Z'; ++c)
                Reader.unreserved[c] = true;

            for (int c = 'a'; c <= 'z'; ++c)
                Reader.unreserved[c] = true;

            Reader.unreserved['-'] = true;
            Reader.unreserved['_'] = true;
            Reader.unreserved['.'] = true;
            Reader.unreserved['!'] = true;
            Reader.unreserved['~'] = true;
            Reader.unreserved['*'] = true;
            Reader.unreserved['\''] = true;
            Reader.unreserved['('] = true;
            Reader.unreserved[')'] = true;
            Reader.unreserved[','] = true;
            Reader.unreserved['"'] = true;
            Reader.unreserved['$'] = true;
            Reader.unreserved[':'] = true;
            Reader.unreserved['@'] = true;
            Reader.unreserved['/'] = true;
            Reader.unreserved['?'] = true;
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

                if (Reader.IsUnreserved(context.Current))
                {
                    node = node.Follow((char)context.Current);

                    isKeyEmpty = false;

                    context.Pull();
                }

                while (Reader.IsUnreserved(context.Current))
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

                if (Reader.IsUnreserved(c) || c == '%')
                    builder.Append((char)c);
                else if (c == '+')
                    builder.Append(' ');
                else
                    break;

                context.Pull();
            }

            value = Uri.UnescapeDataString(builder.ToString());

            return true;
        }

        private void OnError(int position, string message)
        {
            ParserError error;

            error = this.Error;

            if (error != null)
                error(position, message);
        }

        /// <summary>
        /// Check if character is a parameters separator (& or ;).
        /// </summary>
        /// <param name="c">Input character</param>
        /// <returns>True if character is a separator, false otherwise</returns>
        static private bool IsSeparator(int c)
        {
            return c == '&' || c == ';';
        }

        /// <summary>
        /// Check if character is unreserved, i.e. can be used in a query
        /// string without having to escape it.
        /// </summary>
        /// <param name="c">Input character</param>
        /// <remarks>Array is not supported yet (",")</remarks>
        /// <returns>True if character is unreserved, false otherwise</returns>
        static private bool IsUnreserved(int c)
        {
            return c >= 0 && c < Reader.unreserved.Length && Reader.unreserved[c];
        }

        #endregion
    }
}
