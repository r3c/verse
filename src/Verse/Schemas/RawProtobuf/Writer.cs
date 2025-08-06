using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;
using Verse.Formats.RawProtobuf;

namespace Verse.Schemas.RawProtobuf;

internal class Writer(bool noZigZagEncoding) : IWriter<WriterState, RawProtobufValue>
{
    public bool Flush(WriterState state)
    {
        return state.Flush();
    }

    public WriterState Start(Stream stream, ErrorEvent error)
    {
        return new WriterState(stream, error, noZigZagEncoding);
    }

    public void Stop(WriterState state)
    {
    }

    public bool WriteAsArray<TEntity>(WriterState state, IEnumerable<TEntity>? elements,
        WriterCallback<WriterState, RawProtobufValue, TEntity> writer)
    {
        if (elements is null)
            return true;

        foreach (var element in elements)
        {
            var fieldIndex = state.FieldIndex;

            if (!writer(this, state, element))
                return false;

            state.FieldIndex = fieldIndex;
        }

        return true;
    }

    public bool WriteAsObject<TEntity>(WriterState state, TEntity source,
        IReadOnlyDictionary<string, WriterCallback<WriterState, RawProtobufValue, TEntity>> fields)
    {
        var marker = state.ObjectBegin();

        foreach (var field in fields)
        {
            var keySuccess = field.Key.Length > 1 && field.Key[0] == '_'
                ? state.Key(field.Key[1..])
                : state.Key(field.Key);

            if (!keySuccess || !field.Value(this, state, source))
                return false;
        }

        state.ObjectEnd(marker);

        return true;
    }

    public bool WriteAsValue(WriterState state, RawProtobufValue value)
    {
        return state.Value(value);
    }
}