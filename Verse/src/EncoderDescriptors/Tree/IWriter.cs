using System.Collections.Generic;
using System.IO;

namespace Verse.EncoderDescriptors.Tree
{
	internal interface IWriter<TState, TNative>
	{
		void Flush(TState state);

		TState Start(Stream stream, ErrorEvent error);

		void Stop(TState state);

		void WriteAsArray<TElement>(TState state, IEnumerable<TElement> elements,
			WriterCallback<TState, TNative, TElement> callback);

		void WriteAsObject<TObject>(TState state, TObject source,
			IReadOnlyDictionary<string, WriterCallback<TState, TNative, TObject>> fields);

		void WriteAsValue(TState state, TNative value);
	}
}
