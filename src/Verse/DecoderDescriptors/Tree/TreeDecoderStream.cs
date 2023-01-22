namespace Verse.DecoderDescriptors.Tree;

internal class TreeDecoderStream<TState, TNative, TKey, TEntity> : IDecoderStream<TEntity>
{
    private readonly ReaderCallback<TState, TNative, TKey, TEntity> callback;

    private readonly IReader<TState, TNative, TKey> reader;

    private readonly TState state;

    public TreeDecoderStream(IReader<TState, TNative, TKey> reader,
        ReaderCallback<TState, TNative, TKey, TEntity> callback, TState state)
    {
        this.callback = callback;
        this.reader = reader;
        this.state = state;
    }

    public void Dispose()
    {
        reader.Stop(state);
    }

    public bool TryDecode(out TEntity entity)
    {
        var entityValue = default(TEntity);

        var result = callback(reader, state, ref entityValue);

        entity = result == ReaderStatus.Succeeded ? entityValue : default;

        return result != ReaderStatus.Failed;
    }
}