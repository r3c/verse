using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;

namespace Verse.EncoderDescriptors.Recurse
{
	interface IRecurseWriter<TEntity, TState, TValue> : IWriter<TEntity, TState>
	{
		IRecurseWriter<TOther, TState, TValue> Create<TOther>();

		void DeclareArray(WriteEntity<TEntity, TState> enter);

		void DeclareField(string name, WriteEntity<TEntity, TState> enter);

		void DeclareValue(Converter<TEntity, TValue> convert);

		void WriteElements(IEnumerable<TEntity> elements, TState state);
	}
}