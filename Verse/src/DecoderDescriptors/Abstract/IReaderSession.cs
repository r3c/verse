using System.IO;

namespace Verse.DecoderDescriptors.Abstract
{
	interface IReaderSession<TState>
	{
		bool Start(Stream stream, DecodeError error, out TState state);

		void Stop(TState state);
	}
}
