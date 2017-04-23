using System;
using System.IO;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.Schemas.Protobuf.Legacy
{
	class ReaderSession : IReaderSession<ReaderState>
	{
        public bool Start(Stream stream, DecodeError error, out ReaderState state)
        {
            state = new ReaderState(stream, error);

            return true;
        }

        public void Stop(ReaderState state)
        {
        }
	}
}
