using System;
using System.Collections.Generic;

namespace Verse.EncoderDescriptors.Tree
{
	class WriterDefinition<TState, TNative, TEntity>
	{
		public WriterCallback<TState, TNative, TEntity> Callback = (session, state, entity) => session.WriteValue(state, default);

		public virtual WriterDefinition<TState, TNative, TOther> Create<TOther>()
		{
			return new WriterDefinition<TState, TNative, TOther>();
		}
	}
}
