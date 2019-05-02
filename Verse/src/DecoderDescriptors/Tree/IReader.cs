using System;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
	internal interface IReader<TState, TNative, TKey>
	{
		BrowserMove<TElement> ReadToArray<TElement>(TState state, Func<TElement> constructor,
			ReaderCallback<TState, TNative, TKey, TElement> callback);

		bool ReadToObject<TObject>(TState state, ILookup<TKey, ReaderCallback<TState, TNative, TKey, TObject>> lookup,
			ref TObject target);

		bool ReadToValue(TState state, out TNative value);

		TState Start(Stream stream, ErrorEvent error);

		void Stop(TState state);
	}
}
