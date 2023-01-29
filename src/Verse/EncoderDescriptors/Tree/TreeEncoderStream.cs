namespace Verse.EncoderDescriptors.Tree;

internal class TreeEncoderStream<TState, TNative, TEntity> : IEncoderStream<TEntity>
{
    private readonly IWriter<TState, TNative> _reader;

    private readonly TState _state;

    private readonly WriterCallback<TState, TNative, TEntity> _callback;

    public TreeEncoderStream(IWriter<TState, TNative> reader, WriterCallback<TState, TNative, TEntity> callback,
        TState state)
    {
        _callback = callback;
        _reader = reader;
        _state = state;
    }

    public void Dispose()
    {
        _reader.Stop(_state);
    }

    public void Encode(TEntity input)
    {
        _callback(_reader, _state, input);
        _reader.Flush(_state);
    }
}