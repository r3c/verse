using System.Diagnostics.CodeAnalysis;

namespace Verse.DecoderDescriptors.Tree;

internal class TreeDecoderStream<TState, TNative, TKey, TEntity>(
    IReader<TState, TNative, TKey> reader,
    ReaderCallback<TState, TNative, TKey, TEntity> callback,
    TState state)
    : IDecoderStream<TEntity>
{
    public void Dispose()
    {
        reader.Stop(state);
    }

    public bool TryDecode([NotNullWhen(true)] out TEntity? entity)
    {
        var entityValue = default(TEntity)!;
        var result = callback(reader, state, ref entityValue);

        entity = result == ReaderStatus.Succeeded ? entityValue : default;

        return result != ReaderStatus.Failed;
    }
}