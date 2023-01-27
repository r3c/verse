using System.IO;

namespace Verse.EncoderDescriptors.Tree;

internal class TreeEncoder<TState, TNative, TEntity> : IEncoder<TEntity>
{
    public event ErrorEvent? Error;

    private readonly WriterCallback<TState, TNative, TEntity> _callback;

    private readonly IWriter<TState, TNative> _reader;

    public TreeEncoder(IWriter<TState, TNative> reader, WriterCallback<TState, TNative, TEntity> callback)
    {
        _callback = callback;
        _reader = reader;
    }

    public IEncoderStream<TEntity> Open(Stream output)
    {
        var state = _reader.Start(output, (p, m) => Error?.Invoke(p, m));

        return new TreeEncoderStream<TState, TNative, TEntity>(_reader, _callback, state);
    }
}