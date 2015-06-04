namespace Verse.ParserDescriptors.Flat
{
    internal delegate bool Follow<TEntity, TContext, TNative>(ref TEntity target, IReader<TContext, TNative> reader, TContext context);
}