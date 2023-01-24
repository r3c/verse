using System.IO;

namespace Verse.DecoderDescriptors.Tree;

internal class TreeDecoder<TState, TNative, TKey, TEntity> : IDecoder<TEntity>
{
    public event ErrorEvent Error;

    private readonly ReaderCallback<TState, TNative, TKey, TEntity> _callback;

    private readonly IReader<TState, TNative, TKey> _reader;

    public TreeDecoder(IReader<TState, TNative, TKey> reader,
        ReaderCallback<TState, TNative, TKey, TEntity> callback)
    {
        _callback = callback;
        _reader = reader;
    }

    public IDecoderStream<TEntity> Open(Stream input)
    {
        var state = _reader.Start(input, (p, m) => Error?.Invoke(p, m));

        return new TreeDecoderStream<TState, TNative, TKey, TEntity>(_reader, _callback, state);
    }
}