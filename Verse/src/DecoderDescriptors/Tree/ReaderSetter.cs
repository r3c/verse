namespace Verse.DecoderDescriptors.Tree
{
	delegate bool ReaderSetter<TState, TNative, TEntity>(IReaderSession<TState, TNative> session, TState state, ref TEntity target);
}