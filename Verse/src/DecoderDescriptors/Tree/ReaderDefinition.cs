
namespace Verse.DecoderDescriptors.Tree
{
	internal class ReaderDefinition<TState, TNative, TEntity>
	{
		public ReaderCallback<TState, TNative, TEntity> Callback =
			(IReader<TState, TNative> session, TState state, ref TEntity entity) =>
				session.ReadToValue(state, out _);

		public virtual ReaderDefinition<TState, TNative, TOther> Create<TOther>()
		{
			return new ReaderDefinition<TState, TNative, TOther>();
		}
	}
}
