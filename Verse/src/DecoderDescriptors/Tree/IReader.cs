using System;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
	internal interface IReader<TState, TNative, TKey>
	{
		bool ReadToObject<TObject>(TState state, ILookupNode<TKey, ReaderCallback<TState, TNative, TKey, TObject>> root,
			ref TObject target);

		bool ReadToValue(TState state, out TNative value);

		TState Start(Stream stream, ErrorEvent error);

		void Stop(TState state);

		bool TryReadToArray<TElement>(TState state, Func<TElement> constructor,
			ReaderCallback<TState, TNative, TKey, TElement> callback, out BrowserMove<TElement> browserMove);
	}
}
