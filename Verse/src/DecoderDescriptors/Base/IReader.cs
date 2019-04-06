
namespace Verse.DecoderDescriptors.Base
{
	interface IReader<TEntity, in TState>
	{
		bool Read(ref TEntity entity, TState state);
	}
}
