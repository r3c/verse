
namespace Verse.EncoderDescriptors.Base
{
	public interface IWriter<in TState, in TEntity>
	{
		void Write(TState state, TEntity entity);
	}
}
