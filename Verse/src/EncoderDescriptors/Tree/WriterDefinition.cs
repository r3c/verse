
namespace Verse.EncoderDescriptors.Tree
{
	internal class WriterDefinition<TState, TNative, TEntity>
	{
		public WriterCallback<TState, TNative, TEntity> Callback = (session, state, entity) => session.WriteAsValue(state, default);

		public virtual WriterDefinition<TState, TNative, TOther> Create<TOther>()
		{
			return new WriterDefinition<TState, TNative, TOther>();
		}
	}
}
