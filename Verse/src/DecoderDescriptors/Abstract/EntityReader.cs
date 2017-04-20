namespace Verse.DecoderDescriptors.Abstract
{
	delegate bool EntityReader<TEntity, TState>(ref TEntity target, TState state);
}