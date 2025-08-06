using Verse.DecoderDescriptors.Tree;
using Verse.Formats.Json;
using Verse.Lookups;

namespace Verse.Schemas.Json;

internal class ReaderDefinition<TEntity> : IReaderDefinition<ReaderState, JsonValue, int, TEntity>
{
    public ILookup<int, ReaderCallback<ReaderState, JsonValue, int, TEntity>> Lookup { get; } =
        new IndexOrNameLookup<ReaderCallback<ReaderState, JsonValue, int, TEntity>>();

    public IReaderDefinition<ReaderState, JsonValue, int, TOther> Create<TOther>()
    {
        return new ReaderDefinition<TOther>();
    }
}