namespace Verse.DecoderDescriptors.Tree;

internal class TreeDecoderStream<TState, TNative, TKey, TEntity> : IDecoderStream<TEntity>
{
    private readonly ReaderCallback<TState, TNative, TKey, TEntity> _callback;

    private readonly IReader<TState, TNative, TKey> _reader;

    private readonly TState _state;

    public TreeDecoderStream(IReader<TState, TNative, TKey> reader,
        ReaderCallback<TState, TNative, TKey, TEntity> callback, TState state)
    {
        _callback = callback;
        _reader = reader;
        _state = state;
    }

    public void Dispose()
    {
        _reader.Stop(_state);
    }

    public bool TryDecode(out TEntity? entity)
    {
        var entityValue = default(TEntity)!;
        var result = _callback(_reader, _state, ref entityValue);

        entity = result == ReaderStatus.Succeeded ? entityValue : default;

        return result != ReaderStatus.Failed;
    }
}