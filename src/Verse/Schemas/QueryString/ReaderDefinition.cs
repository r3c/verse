using Verse.DecoderDescriptors.Tree;
using Verse.Lookups;

namespace Verse.Schemas.QueryString;

internal class ReaderDefinition<TEntity> : IReaderDefinition<ReaderState, string, char, TEntity>
{
    public ILookup<char, ReaderCallback<ReaderState, string, char, TEntity>> Lookup { get; } =
        new NameLookup<ReaderCallback<ReaderState, string, char, TEntity>>();

    public IReaderDefinition<ReaderState, string, char, TOther> Create<TOther>()
    {
        return new ReaderDefinition<TOther>();
    }
}