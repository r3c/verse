using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;
using Verse.Formats.RawProtobuf;

namespace Verse.Schemas.RawProtobuf;

internal class WriterDefinition<TEntity> : IWriterDefinition<WriterState, RawProtobufValue, TEntity>
{
    public Dictionary<string, WriterCallback<WriterState, RawProtobufValue, TEntity>> Fields { get; } = new();

    public IWriterDefinition<WriterState, RawProtobufValue, TOther> Create<TOther>()
    {
        return new WriterDefinition<TOther>();
    }
}