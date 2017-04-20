namespace Verse.EncoderDescriptors.Abstract
{
	delegate void EntityWriter<TEntity, TState>(TEntity source, TState state);
}