using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;
using Verse.Formats.Json;

namespace Verse.Schemas.Json;

internal class WriterDefinition<TEntity> : IWriterDefinition<WriterState, JsonValue, TEntity>
{
    public Dictionary<string, WriterCallback<WriterState, JsonValue, TEntity>> Fields { get; } = new();

    public IWriterDefinition<WriterState, JsonValue, TOther> Create<TOther>()
    {
        return new WriterDefinition<TOther>();
    }
}