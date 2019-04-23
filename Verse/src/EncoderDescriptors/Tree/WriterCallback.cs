namespace Verse.EncoderDescriptors.Tree
{
	internal delegate void WriterCallback<TState, TNative, in TEntity>(IWriterSession<TState, TNative> session, TState state, TEntity source);
}