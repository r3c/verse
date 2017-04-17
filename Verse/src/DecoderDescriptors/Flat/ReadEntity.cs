namespace Verse.DecoderDescriptors.Flat
{
	delegate bool ReadEntity<TEntity, TState>(ref TEntity target, TState state);
}