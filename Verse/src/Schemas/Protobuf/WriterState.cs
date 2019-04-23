using System.IO;

namespace Verse.Schemas.Protobuf
{
    internal class WriterState
    {
        public readonly EncodeError Error;

        public readonly Stream Stream;        

        public WriterState(Stream stream, EncodeError error)
        {
            this.Error = error;
            this.Stream = stream;
        }
    }
}
