namespace Verse.EncoderDescriptors.Abstract
{
	delegate void EntityWriter<in TEntity, in TState>(TEntity source, TState state);
}