using System.IO;
using Verse.DecoderDescriptors.Base;

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
