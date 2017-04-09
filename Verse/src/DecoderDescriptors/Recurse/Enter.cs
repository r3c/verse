namespace Verse.DecoderDescriptors.Recurse
{
    delegate bool Enter<TEntity, TState>(ref TEntity target, TState state);
}