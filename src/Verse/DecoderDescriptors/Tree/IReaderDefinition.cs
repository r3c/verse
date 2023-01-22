namespace Verse.DecoderDescriptors.Tree;

internal interface IReaderDefinition<TState, TNative, TKey, TEntity>
{
    ReaderCallback<TState, TNative, TKey, TEntity> Callback { get; set; }

    ILookup<TKey, ReaderCallback<TState, TNative, TKey, TEntity>> Lookup { get; }

    IReaderDefinition<TState, TNative, TKey, TOther> Create<TOther>();
}