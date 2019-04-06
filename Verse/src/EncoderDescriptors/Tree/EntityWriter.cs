namespace Verse.EncoderDescriptors.Tree
{
	delegate void EntityWriter<in TState, in TEntity>(TState state, TEntity source);
}