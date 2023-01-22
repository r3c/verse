using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf;

internal class WriterDefinition<TEntity> : IWriterDefinition<WriterState, RawProtobufValue, TEntity>
{
    public WriterCallback<WriterState, RawProtobufValue, TEntity> Callback { get; set; } =
        (reader, state, entity) => reader.WriteAsValue(state, new RawProtobufValue());

    public IWriterDefinition<WriterState, RawProtobufValue, TOther> Create<TOther>()
    {
        return new WriterDefinition<TOther>();
    }
}