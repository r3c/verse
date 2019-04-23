namespace Verse.DecoderDescriptors.Tree
{
	internal delegate bool ReaderCallback<TState, TNative, TEntity>(IReader<TState, TNative> session,
		TState state, ref TEntity target);
}