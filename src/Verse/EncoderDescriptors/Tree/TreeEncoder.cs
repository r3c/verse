using System.IO;

namespace Verse.EncoderDescriptors.Tree;

internal class TreeEncoder<TState, TNative, TEntity>(
    IWriter<TState, TNative> reader,
    WriterCallback<TState, TNative, TEntity> callback)
    : IEncoder<TEntity>
{
    public event ErrorEvent? Error;

    public IEncoderStream<TEntity> Open(Stream output)
    {
        var state = reader.Start(output, (p, m) => Error?.Invoke(p, m));

        return new TreeEncoderStream<TState, TNative, TEntity>(reader, callback, state);
    }
}