using System;

namespace Verse.DecoderDescriptors.Tree
{
	delegate bool ArrayReader<TState, TEntity>(TState state, Func<TEntity> constructor, out TEntity entity);
}