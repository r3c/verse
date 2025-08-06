using System;
using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;
using Verse.Formats.Protobuf;

namespace Verse.Schemas.Protobuf;

internal class Writer : IWriter<WriterState, ProtobufValue>
{
    public bool Flush(WriterState state)
    {
        throw new NotImplementedException();
    }

    public WriterState Start(Stream stream, ErrorEvent error)
    {
        throw new NotImplementedException();
    }

    public void Stop(WriterState state)
    {
        throw new NotImplementedException();
    }

    public bool WriteAsArray<TEntity>(WriterState state, IEnumerable<TEntity>? elements,
        WriterCallback<WriterState, ProtobufValue, TEntity> writer)
    {
        throw new NotImplementedException();
    }

    public bool WriteAsObject<TEntity>(WriterState state, TEntity entity,
        IReadOnlyDictionary<string, WriterCallback<WriterState, ProtobufValue, TEntity>> fields)
    {
        throw new NotImplementedException();
    }

    public bool WriteAsValue(WriterState state, ProtobufValue value)
    {
        throw new NotImplementedException();
    }
}