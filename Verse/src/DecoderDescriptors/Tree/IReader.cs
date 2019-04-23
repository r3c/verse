using System;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
	internal interface IReader<TState, TNative>
	{
		BrowserMove<TElement> ReadToArray<TElement>(TState state, Func<TElement> constructor,
			ReaderCallback<TState, TNative, TElement> callback);

		bool ReadToObject<TObject>(TState state, ILookup<int, ReaderCallback<TState, TNative, TObject>> fields,
			ref TObject target);

		bool ReadToValue(TState state, out TNative value);

		TState Start(Stream stream, ErrorEvent error);

		void Stop(TState state);
	}
}
