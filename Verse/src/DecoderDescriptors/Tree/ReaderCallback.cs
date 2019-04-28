namespace Verse.DecoderDescriptors.Tree
{
	internal delegate bool ReaderCallback<TState, TNative, TEntity>(IReader<TState, TNative> reader,
		TState state, ref TEntity target);
}