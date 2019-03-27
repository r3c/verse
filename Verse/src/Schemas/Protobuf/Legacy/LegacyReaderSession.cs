using System.IO;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.Schemas.Protobuf.Legacy
{
	class LegacyReaderSession : IReaderSession<LegacyReaderState>
	{
		public bool Start(Stream stream, DecodeError error, out LegacyReaderState state)
		{
			state = new LegacyReaderState(stream, error);

			return true;
		}

		public void Stop(LegacyReaderState state)
		{
		}
	}
}
