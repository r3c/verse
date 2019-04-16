using System;

namespace Verse.DecoderDescriptors.Tree
{
	class ReaderDefinition<TState, TNative, TEntity>
	{
		public ReaderCallback<TState, TNative, TEntity> Callback = (IReaderSession<TState, TNative> session, TState state, out TEntity entity) =>
		{
			entity = default;

			return session.ReadToValue(state, out _);
		};

		public virtual ReaderDefinition<TState, TNative, TOther> Create<TOther>()
		{
			return new ReaderDefinition<TState, TNative, TOther>();
		}
	}
}
