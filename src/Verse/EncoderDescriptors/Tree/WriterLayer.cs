namespace Verse.EncoderDescriptors.Tree;

internal record WriterLayer<TState, TNative, TEntity>(IWriterDefinition<TState, TNative, TEntity> Definition)
{
    public WriterCallback<TState, TNative, TEntity> Callback = (_, _, _) => false;
}