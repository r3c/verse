namespace Verse.EncoderDescriptors.Base
{
	delegate void EntityWriter<in TEntity, in TState>(TEntity source, TState state);
}