using System;
using System.IO;

namespace Verse.Schemas.Protobuf
{
    class WriterState
    {
        #region Attributes / Public

        public readonly EncodeError Error;

        public readonly Stream Stream;        

        #endregion

        #region Constructors

        public WriterState(Stream stream, EncodeError error)
        {
            this.Error = error;
            this.Stream = stream;
        }

        #endregion
    }
}
