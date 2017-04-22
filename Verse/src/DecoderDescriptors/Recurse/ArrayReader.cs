using System;

namespace Verse.DecoderDescriptors.Recurse
{
	delegate bool ArrayReader<TEntity, TState>(ref TEntity entity, TState state);
}
