namespace Verse.DecoderDescriptors.Tree
{
	delegate bool ReaderCallback<TState, TNative, TEntity>(IReaderSession<TState, TNative> session, TState state, out TEntity target);
}