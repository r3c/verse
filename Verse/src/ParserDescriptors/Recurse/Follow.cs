namespace Verse.ParserDescriptors.Recurse
{
    internal delegate bool Follow<TEntity, TContext, TNative>(ref TEntity target, IReader<TContext, TNative> reader, TContext context);
}