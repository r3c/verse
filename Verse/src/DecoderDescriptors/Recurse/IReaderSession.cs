using System;
using System.IO;

namespace Verse.DecoderDescriptors.Recurse
{
	interface IReaderSession<TState>
	{
		bool Start(Stream stream, DecodeError error, out TState state);

		void Stop(TState state);
	}
}
