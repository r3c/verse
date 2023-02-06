using Verse.DecoderDescriptors.Tree;
using Verse.Formats.Json;
using Verse.Lookups;

namespace Verse.Schemas.Json;

internal class ReaderDefinition<TEntity> : IReaderDefinition<ReaderState, JsonValue, int, TEntity>
{
    public ReaderCallback<ReaderState, JsonValue, int, TEntity> Callback { get; set; } =
        (IReader<ReaderState, JsonValue, int> reader, ReaderState state, ref TEntity entity) =>
            reader.ReadToValue(state, out _);

    public ILookup<int, ReaderCallback<ReaderState, JsonValue, int, TEntity>> Lookup { get; } =
        new IndexOrNameLookup<ReaderCallback<ReaderState, JsonValue, int, TEntity>>();

    public IReaderDefinition<ReaderState, JsonValue, int, TOther> Create<TOther>()
    {
        return new ReaderDefinition<TOther>();
    }
}