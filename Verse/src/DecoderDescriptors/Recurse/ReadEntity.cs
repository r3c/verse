namespace Verse.DecoderDescriptors.Recurse
{
	delegate bool ReadEntity<TEntity, TState>(ref TEntity target, TState state);
}