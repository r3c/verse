using System;

namespace Verse.DecoderDescriptors.Base
{
	interface IReader<in TState, TEntity>
	{
		bool Read(TState state, Func<TEntity> constructor, out TEntity entity);
	}
}
