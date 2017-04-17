using System;
using System.IO;

namespace Verse.DecoderDescriptors.Recurse
{
	interface IReader<TEntity, TValue, TState>
	{
		#region Properties

		bool HoldArray
		{
			get;
		}

		bool HoldValue
		{
			get;
		}

		#endregion

		#region Methods

		IReader<TOther, TValue, TState> Create<TOther>();

		void DeclareArray(Enter<TEntity, TState> enter);

		void DeclareField(string name, Enter<TEntity, TState> enter);

		void DeclareValue(DecodeAssign<TEntity, TValue> assign);

		IBrowser<TEntity> ReadElements(Func<TEntity> constructor, TState state);

		bool ReadEntity(Func<TEntity> constructor, TState state, out TEntity target);

		bool Start(Stream stream, DecodeError error, out TState state);

		void Stop(TState state);

		#endregion
	}
}
