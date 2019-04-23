namespace Verse.EncoderDescriptors.Tree
{
	internal delegate void WriterCallback<TState, TNative, in TEntity>(IWriter<TState, TNative> session, TState state, TEntity source);
}