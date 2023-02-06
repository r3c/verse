using System.Collections.Generic;
using System.IO;
using System.Text;
using Verse.EncoderDescriptors.Tree;
using Verse.Formats.Json;

namespace Verse.Schemas.Json;

internal class Writer : IWriter<WriterState, JsonValue>
{
    private readonly Encoding _encoding;

    private readonly bool _omitNull;

    public Writer(Encoding encoding, bool omitNull)
    {
        _encoding = encoding;
        _omitNull = omitNull;
    }

    public void Flush(WriterState state)
    {
        state.Flush();
    }

    public WriterState Start(Stream stream, ErrorEvent error)
    {
        return new WriterState(stream, _encoding, _omitNull);
    }

    public void Stop(WriterState state)
    {
        state.Dispose();
    }

    public void WriteAsArray<TElement>(WriterState state, IEnumerable<TElement> elements,
        WriterCallback<WriterState, JsonValue, TElement> writer)
    {
        if (elements == null)
            WriteAsValue(state, JsonValue.Undefined);
        else
        {
            state.ArrayBegin();

            foreach (var element in elements)
                writer(this, state, element);

            state.ArrayEnd();
        }
    }

    public void WriteAsObject<TObject>(WriterState state, TObject parent,
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
    }

    public void WriteAsValue(WriterState state, JsonValue value)
    {
        state.Value(value);
    }
}