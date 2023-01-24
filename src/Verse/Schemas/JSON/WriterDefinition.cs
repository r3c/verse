using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.JSON;

internal class WriterDefinition<TEntity> : IWriterDefinition<WriterState, JSONValue, TEntity>
{
    public WriterCallback<WriterState, JSONValue, TEntity> Callback { get; set; } = (reader, state, _) =>
        reader.WriteAsValue(state, JSONValue.Void);

    public Dictionary<string, WriterCallback<WriterState, JSONValue, TEntity>> Fields { get; } = new();

    public IWriterDefinition<WriterState, JSONValue, TOther> Create<TOther>()
    {
        return new WriterDefinition<TOther>();
    }
}