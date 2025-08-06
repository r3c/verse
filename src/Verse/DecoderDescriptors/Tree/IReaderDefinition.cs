namespace Verse.DecoderDescriptors.Tree;

internal interface IReaderDefinition<TState, TNative, TKey, TEntity>
{
    ILookup<TKey, ReaderCallback<TState, TNative, TKey, TEntity>> Lookup { get; }

    IReaderDefinition<TState, TNative, TKey, TOther> Create<TOther>();
}