namespace Verse.DecoderDescriptors.Tree
{
	internal delegate bool ReaderCallback<TState, TNative, TKey, TEntity>(IReader<TState, TNative, TKey> reader,
		TState state, ref TEntity target);
}