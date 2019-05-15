namespace Verse.DecoderDescriptors.Tree
{
	internal delegate ReaderStatus ReaderCallback<TState, TNative, TKey, TEntity>(IReader<TState, TNative, TKey> reader,
		TState state, ref TEntity target);
}