using System;

namespace Verse.DecoderDescriptors.Abstract
{
	interface IReader<TEntity, TState>
	{
		bool Read(ref TEntity entity, TState state);
	}
}
