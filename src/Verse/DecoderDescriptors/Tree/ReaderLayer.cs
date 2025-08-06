namespace Verse.DecoderDescriptors.Tree;

internal record ReaderLayer<TState, TNative, TKey, TEntity>(
    IReaderDefinition<TState, TNative, TKey, TEntity> Definition)
{
    public ReaderCallback<TState, TNative, TKey, TEntity> Callback =
        (IReader<TState, TNative, TKey> _, TState _, ref TEntity _) => ReaderStatus.Failed;
}