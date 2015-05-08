namespace Verse.PrinterDescriptors.Recurse
{
    internal delegate void Follow<TEntity, TContext, TNative>(TEntity source, IWriter<TContext, TNative> writer, TContext context);
}