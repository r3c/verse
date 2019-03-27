using System.Globalization;
using System.IO;

namespace Verse.Schemas.Protobuf
{
    class ReaderState
    {
        public readonly Stream Stream;

        public ProtobufValue Value;

        private readonly DecodeError error;

        public ReaderState(Stream stream, DecodeError error)
        {
            this.error = error;
            this.Stream = stream;
            this.Value = ProtobufValue.Void;
        }

        public void RaiseError(string format, params object[] args)
        {
            this.error((int)this.Stream.Position, string.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}
