
namespace Verse.EncoderDescriptors.Tree
{
	internal class WriterDefinition<TState, TNative, TEntity>
	{
		public WriterCallback<TState, TNative, TEntity> Callback = (reader, state, entity) =>
			reader.WriteAsValue(state, default);

		public virtual WriterDefinition<TState, TNative, TOther> Create<TOther>()
		{
			return new WriterDefinition<TState, TNative, TOther>();
		}
	}
}
