using System;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.DecoderDescriptors.Flat
{
	interface IFlatReader<TEntity, TState, TValue> : IReader<TEntity, TState>
	{
		IFlatReader<TOther, TState, TValue> Create<TOther>();

		void DeclareField(string name, ReadEntity<TEntity, TState> enter);

		void DeclareValue(Converter<TValue, TEntity> convert);

		bool ReadValue(Func<TEntity> constructor, TState state, out TEntity target);
	}
}