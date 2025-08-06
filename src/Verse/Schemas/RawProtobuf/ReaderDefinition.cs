using Verse.DecoderDescriptors.Tree;
using Verse.Formats.RawProtobuf;
using Verse.Lookups;

namespace Verse.Schemas.RawProtobuf;

internal class ReaderDefinition<TEntity> : IReaderDefinition<ReaderState, RawProtobufValue, char, TEntity>
{
    public ILookup<char, ReaderCallback<ReaderState, RawProtobufValue, char, TEntity>> Lookup { get; } =
        new NameLookup<ReaderCallback<ReaderState, RawProtobufValue, char, TEntity>>();

    public IReaderDefinition<ReaderState, RawProtobufValue, char, TOther> Create<TOther>()
    {
        return new ReaderDefinition<TOther>();
    }
}