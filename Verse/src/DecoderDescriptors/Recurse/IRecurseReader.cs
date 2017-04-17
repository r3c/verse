using System;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.DecoderDescriptors.Recurse
{
	interface IRecurseReader<TEntity, TState, TValue> : IReader<TEntity, TState>
	{
		IRecurseReader<TOther, TState, TValue> Create<TOther>();

		void DeclareArray(ReadArray<TEntity, TState> read);

		void DeclareField(string name, ReadEntity<TEntity, TState> enter);

		void DeclareValue(Converter<TValue, TEntity> convert);

		BrowserMove<TEntity> ReadElements(Func<TEntity> constructor, TState state);
	}
}
