using System;

namespace Verse.LookupNodes;

internal class EmptyLookupNode<TKey, TValue> : ILookupNode<TKey, TValue>
{
    public static readonly EmptyLookupNode<TKey, TValue> Instance = new();

    public bool HasValue => false;
    public TValue Value => throw new InvalidOperationException();

    public ILookupNode<TKey, TValue> Follow(TKey key)
    {
        return this;
    }
}