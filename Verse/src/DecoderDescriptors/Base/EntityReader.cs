namespace Verse.DecoderDescriptors.Base
{
	delegate bool EntityReader<in TState, TEntity>(TState state, ref TEntity target);
}