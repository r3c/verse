
using Verse.Lookups;

namespace Verse.DecoderDescriptors.Tree
{
	internal class ReaderDefinition<TState, TNative, TEntity>
	{
		public ReaderCallback<TState, TNative, TEntity> Callback =
			(IReader<TState, TNative> reader, TState state, ref TEntity entity) =>
				reader.ReadToValue(state, out _);

		public readonly ILookup<int, ReaderCallback<TState, TNative, TEntity>> Fields =
			new NameLookup<ReaderCallback<TState, TNative, TEntity>>();

		public virtual ReaderDefinition<TState, TNative, TOther> Create<TOther>()
		{
			return new ReaderDefinition<TState, TNative, TOther>();
		}
	}
}
