namespace Verse.DecoderDescriptors.Base
{
	delegate bool EntityReader<TEntity, TState>(ref TEntity target, TState state);
}