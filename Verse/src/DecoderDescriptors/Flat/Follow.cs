namespace Verse.DecoderDescriptors.Flat
{
	internal delegate bool Follow<TEntity, TContext, TNative>(ref TEntity target, IReader<TContext, TNative> reader, TContext context);
}