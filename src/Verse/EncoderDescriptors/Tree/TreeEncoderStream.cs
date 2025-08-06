namespace Verse.EncoderDescriptors.Tree;

internal class TreeEncoderStream<TState, TNative, TEntity>(
    IWriter<TState, TNative> reader,
    WriterCallback<TState, TNative, TEntity> callback,
    TState state)
    : IEncoderStream<TEntity>
{
    public void Dispose()
    {
        reader.Stop(state);
    }

    public bool Encode(TEntity input)
    {
        return callback(reader, state, input) && reader.Flush(state);
    }
}