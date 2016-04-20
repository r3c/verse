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

            this.Pull();
        }

        #endregion

        #region Methods

        public void Pull()
        {
            this.Current = this.reader.Read();

            ++this.Position;
        }

        #endregion
    }
}