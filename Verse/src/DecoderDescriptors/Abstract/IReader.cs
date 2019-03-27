
namespace Verse.DecoderDescriptors.Abstract
{
	interface IReader<TEntity, in TState>
	{
		bool Read(ref TEntity entity, TState state);
	}
}
