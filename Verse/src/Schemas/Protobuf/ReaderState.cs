using System;
using System.Globalization;
using System.IO;

namespace Verse.Schemas.Protobuf
{
    class ReaderState
    {
        #region Attributes / Public

        public readonly Stream Stream;

        public ProtobufValue Value;

        #endregion

        #region Attributes / Private

        private readonly DecodeError error;

        #endregion

        #region Constructors

        public ReaderState(Stream stream, DecodeError error)
        {
            this.error = error;
            this.Stream = stream;
            this.Value = ProtobufValue.Void;
        }

        #endregion

        #region Methods

        public void RaiseError(string format, params object[] args)
        {
            this.error((int)this.Stream.Position, string.Format(CultureInfo.InvariantCulture, format, args));
        }

        #endregion
    }
}
