namespace Verse.EncoderDescriptors.Tree
{
	delegate void WriterCallback<TState, TNative, in TEntity>(IWriterSession<TState, TNative> session, TState state, TEntity source);
}