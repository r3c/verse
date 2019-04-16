using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
	interface IReaderSession<TState, TNative>
	{
		BrowserMove<TElement> ReadToArray<TElement>(TState state, ReaderCallback<TState, TNative, TElement> callback);

		bool ReadToObject<TObject>(TState state, EntityTree<ReaderSetter<TState, TNative, TObject>> fields, ref TObject target);

		bool ReadToValue(TState state, out TNative value);

		bool Start(Stream stream, DecodeError error, out TState state);

		void Stop(TState state);
	}
}
