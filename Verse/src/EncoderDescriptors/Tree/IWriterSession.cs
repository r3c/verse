using System.Collections.Generic;
using System.IO;

namespace Verse.EncoderDescriptors.Tree
{
	interface IWriterSession<TState, TNative>
	{
		bool Start(Stream stream, EncodeError error, out TState state);

		void Stop(TState state);

		void WriteArray<TElement>(TState state, IEnumerable<TElement> elements, WriterCallback<TState, TNative, TElement> callback);

		void WriteObject<TObject>(TState state, TObject source, IReadOnlyDictionary<string, WriterCallback<TState, TNative, TObject>> fields);

		void WriteValue(TState state, TNative value);
	}
}
