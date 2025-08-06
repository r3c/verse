using System.IO;

namespace Verse.DecoderDescriptors.Tree;

internal class TreeDecoder<TState, TNative, TKey, TEntity>(
    IReader<TState, TNative, TKey> reader,
    ReaderCallback<TState, TNative, TKey, TEntity> callback)
    : IDecoder<TEntity>
{
    public event ErrorEvent? Error;

    public IDecoderStream<TEntity> Open(Stream input)
    {
        var state = reader.Start(input, (p, m) => Error?.Invoke(p, m));

        return new TreeDecoderStream<TState, TNative, TKey, TEntity>(reader, callback, state);
    }
}