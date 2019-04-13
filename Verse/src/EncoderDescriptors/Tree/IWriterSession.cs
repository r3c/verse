using System.Collections.Generic;
using System.IO;

namespace Verse.EncoderDescriptors.Tree
{
	interface IWriterSession<TState, TNative>
	{
		bool Start(Stream stream, EncodeError error, out TState state);

		void Stop(TState state);

		void WriteArray<TEntity>(TState state, IEnumerable<TEntity> elements, WriterCallback<TState, TNative, TEntity> callback);

		void WriteObject<TEntity>(TState state, TEntity parent, IReadOnlyDictionary<string, WriterCallback<TState, TNative, TEntity>> fields);

		void WriteValue(TState state, TNative value);
	}
}
