using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;
using Verse.Formats.Protobuf;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf;

internal class ProtobufWriterDefinition<TEntity>(ProtoBinding[] fields) :
    IWriterDefinition<WriterState, ProtobufValue, TEntity>
{
    public Dictionary<string, WriterCallback<WriterState, ProtobufValue, TEntity>> Fields { get; } = new();

    public IWriterDefinition<WriterState, ProtobufValue, TOther> Create<TOther>()
    {
        return new ProtobufWriterDefinition<TOther>(fields);
    }
}