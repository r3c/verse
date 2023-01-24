﻿using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf;

internal class ProtobufWriterDefinition<TEntity> : IWriterDefinition<WriterState, ProtobufValue, TEntity>
{
    private readonly ProtoBinding[] fields;

    public ProtobufWriterDefinition(ProtoBinding[] fields)
    {
        this.fields = fields;
    }

    public WriterCallback<WriterState, ProtobufValue, TEntity> Callback { get; set; } = (reader, state, entity) =>
        reader.WriteAsValue(state, ProtobufValue.Empty);

    public Dictionary<string, WriterCallback<WriterState, ProtobufValue, TEntity>> Fields { get; } = new();

    public IWriterDefinition<WriterState, ProtobufValue, TOther> Create<TOther>()
    {
        return new ProtobufWriterDefinition<TOther>(fields);
    }
}