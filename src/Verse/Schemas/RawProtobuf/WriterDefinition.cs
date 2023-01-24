using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf;

internal class WriterDefinition<TEntity> : IWriterDefinition<WriterState, RawProtobufValue, TEntity>
{
    public WriterCallback<WriterState, RawProtobufValue, TEntity> Callback { get; set; } =
        (reader, state, _) => reader.WriteAsValue(state, new RawProtobufValue());

    public Dictionary<string, WriterCallback<WriterState, RawProtobufValue, TEntity>> Fields { get; } = new();

    public IWriterDefinition<WriterState, RawProtobufValue, TOther> Create<TOther>()
    {
        return new WriterDefinition<TOther>();
    }
}