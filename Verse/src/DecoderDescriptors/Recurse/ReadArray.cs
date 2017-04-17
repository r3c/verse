using System;

namespace Verse.DecoderDescriptors.Recurse
{
	delegate bool ReadArray<TEntity, TState>(Func<TEntity> constructor, TState state, out TEntity entity);
}
