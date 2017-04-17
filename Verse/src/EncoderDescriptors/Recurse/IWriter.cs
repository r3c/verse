using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.EncoderDescriptors.Recurse
{
	interface IWriter<TEntity, TValue, TState>
	{
		#region Methods

		IWriter<TOther, TValue, TState> Create<TOther>();

		void DeclareArray(Enter<TEntity, TState> enter);

		void DeclareField(string name, Enter<TEntity, TState> enter);

		void DeclareValue(Converter<TEntity, TValue> convert);

		void WriteElements(IEnumerable<TEntity> elements, TState state);

		void WriteEntity(TEntity source, TState state);

		#endregion
	}
}