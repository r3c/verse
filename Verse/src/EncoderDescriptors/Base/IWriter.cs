
namespace Verse.EncoderDescriptors.Base
{
	public interface IWriter<in TEntity, in TState>
	{
		void WriteEntity(TEntity source, TState state);
	}
}
