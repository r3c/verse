using System;
using System.IO;
using Verse.DecoderDescriptors.Recurse;

namespace Verse.Schemas.Protobuf
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
