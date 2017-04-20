using System;

namespace Verse.DecoderDescriptors.Recurse
{
	delegate bool ArrayReader<TEntity, TState>(Func<TEntity> constructor, TState state, out TEntity entity);
}
