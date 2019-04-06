using System.IO;

namespace Verse.DecoderDescriptors.Base
{
	interface IReaderSession<TState>
	{
		bool Start(Stream stream, DecodeError error, out TState state);

		void Stop(TState state);
	}
}
