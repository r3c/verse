using System;
using System.IO;
using Verse.EncoderDescriptors.Base;

namespace Verse.Schemas.Protobuf.Legacy
{
	class WriterSession : IWriterSession<WriterState>
	{
        public bool Start(Stream stream, EncodeError error, out WriterState state)
        {
            throw new NotImplementedException();
        }
    
        public void Stop(WriterState state)
        {
            throw new NotImplementedException();
        }
	}
}
