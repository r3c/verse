using System;
using System.IO;
using Verse.EncoderDescriptors.Abstract;

namespace Verse.Schemas.Protobuf
{
	class WriterSession : IWriterSession<WriterState>
	{
		public bool Start(Stream stream, EncodeError error, out WriterState state)
		{
			state = new WriterState(stream, error);

			return true;
		}

		public void Stop(WriterState state)
		{
		}
	}
}
