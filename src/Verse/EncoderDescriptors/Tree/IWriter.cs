using System.Collections.Generic;
using System.IO;

namespace Verse.EncoderDescriptors.Tree;

internal interface IWriter<TState, TNative>
{
    bool Flush(TState state);

    TState Start(Stream stream, ErrorEvent error);

    void Stop(TState state);

    bool WriteAsArray<TElement>(TState state, IEnumerable<TElement>? elements,
        WriterCallback<TState, TNative, TElement> callback);

    bool WriteAsObject<TObject>(TState state, TObject source,
        IReadOnlyDictionary<string, WriterCallback<TState, TNative, TObject>> fields);

    bool WriteAsValue(TState state, TNative value);
}