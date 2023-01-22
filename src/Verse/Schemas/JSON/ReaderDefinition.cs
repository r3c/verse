using Verse.DecoderDescriptors.Tree;
using Verse.Lookups;

namespace Verse.Schemas.JSON;

internal class ReaderDefinition<TEntity> : IReaderDefinition<ReaderState, JSONValue, int, TEntity>
{
    public ReaderCallback<ReaderState, JSONValue, int, TEntity> Callback { get; set; } =
        (IReader<ReaderState, JSONValue, int> reader, ReaderState state, ref TEntity entity) =>
            reader.ReadToValue(state, out _);

    public ILookup<int, ReaderCallback<ReaderState, JSONValue, int, TEntity>> Lookup { get; } =
        new IndexOrNameLookup<ReaderCallback<ReaderState, JSONValue, int, TEntity>>();

    public IReaderDefinition<ReaderState, JSONValue, int, TOther> Create<TOther>()
    {
        return new ReaderDefinition<TOther>();
    }
}