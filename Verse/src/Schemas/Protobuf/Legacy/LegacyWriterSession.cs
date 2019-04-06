using System.IO;
using Verse.EncoderDescriptors.Base;

namespace Verse.Schemas.Protobuf.Legacy
{
	class LegacyWriterSession : IWriterSession<LegacyWriterState>
	{
		public bool Start(Stream stream, EncodeError error, out LegacyWriterState state)
		{
			state = new LegacyWriterState(stream, error);

			return true;
		}

		public void Stop(LegacyWriterState state)
		{
		}
	}
}
