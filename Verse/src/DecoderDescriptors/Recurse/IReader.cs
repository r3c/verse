using System;
using System.IO;

namespace Verse.DecoderDescriptors.Recurse
{
	interface IReader<TEntity, TValue, TState>
	{
		#region Methods

		IReader<TOther, TValue, TState> Create<TOther>();

		void DeclareArray(ReadArray<TEntity, TState> read);

		void DeclareField(string name, ReadEntity<TEntity, TState> enter);

		void DeclareValue(Converter<TValue, TEntity> convert);

		BrowserMove<TEntity> ReadElements(Func<TEntity> constructor, TState state);

		bool ReadEntity(Func<TEntity> constructor, TState state, out TEntity entity);

		bool Start(Stream stream, DecodeError error, out TState state);

		void Stop(TState state);

		#endregion
	}
}
