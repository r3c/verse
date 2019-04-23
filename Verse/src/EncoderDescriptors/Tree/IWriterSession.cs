using System.Collections.Generic;
using System.IO;

namespace Verse.EncoderDescriptors.Tree
{
	internal interface IWriterSession<TState, TNative>
	{
		TState Start(Stream stream, ErrorEvent error);

		void Stop(TState state);

		void WriteArray<TElement>(TState state, IEnumerable<TElement> elements, WriterCallback<TState, TNative, TElement> callback);

		void WriteObject<TObject>(TState state, TObject source, IReadOnlyDictionary<string, WriterCallback<TState, TNative, TObject>> fields);

		void WriteValue(TState state, TNative value);
	}
}
