
namespace Verse.DecoderDescriptors.Base
{
	interface IReader<in TState, TEntity>
	{
		bool Read(ref TEntity entity, TState state);
	}
}
