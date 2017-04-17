using System;

namespace Verse.DecoderDescriptors.Abstract
{
	interface IReader<TEntity, TState>
	{
		bool ReadEntity(Func<TEntity> constructor, TState state, out TEntity entity);
	}
}
