using System.Collections.Generic;
using System.IO;
using System.Text;
using Verse.EncoderDescriptors.Tree;
using Verse.Formats.Json;

namespace Verse.Schemas.Json;

internal class Writer(Encoding encoding, bool omitNull) : IWriter<WriterState, JsonValue>
{
    public bool Flush(WriterState state)
    {
        return state.Flush();
    }

    public WriterState Start(Stream stream, ErrorEvent error)
    {
        return new WriterState(stream, encoding, omitNull);
    }

    public void Stop(WriterState state)
    {
        state.Dispose();
    }

    public bool WriteAsArray<TElement>(WriterState state, IEnumerable<TElement>? elements,
        WriterCallback<WriterState, JsonValue, TElement> writer)
    {
        if (elements is null)
            WriteAsValue(state, JsonValue.Undefined);
        else
        {
            state.ArrayBegin();

            foreach (var element in elements)
                writer(this, state, element);

            state.ArrayEnd();
        }

        return true;
    }

    public bool WriteAsObject<TObject>(WriterState state, TObject parent,
        IReadOnlyDictionary<string, WriterCallback<WriterState, JsonValue, TObject>> fields)
    {
        if (parent == null)
            WriteAsValue(state, JsonValue.Undefined);
        else
        {
            state.ObjectBegin();

            foreach (var field in fields)
            {
                state.Key(field.Key);
                field.Value(this, state, parent);
            }

            state.ObjectEnd();
        }

        return true;
    }

    public bool WriteAsValue(WriterState state, JsonValue value)
    {
        return state.Value(value);
    }
}