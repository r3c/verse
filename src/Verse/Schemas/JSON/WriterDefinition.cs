using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.JSON;

internal class WriterDefinition<TEntity> : IWriterDefinition<WriterState, JSONValue, TEntity>
{
    public WriterCallback<WriterState, JSONValue, TEntity> Callback { get; set; } = (reader, state, entity) =>
        reader.WriteAsValue(state, JSONValue.Void);

    public IWriterDefinition<WriterState, JSONValue, TOther> Create<TOther>()
    {
        return new WriterDefinition<TOther>();
    }
}