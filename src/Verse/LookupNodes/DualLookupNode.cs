using System;

namespace Verse.LookupNodes;

internal class DualLookupNode<TKey, TValue>(ILookupNode<TKey, TValue> shortcut, ILookupNode<TKey, TValue> fallback)
    : ILookupNode<TKey, TValue>
{
    public bool HasValue => shortcut.HasValue || fallback.HasValue;

    public TValue Value
    {
        get
        {
            if (shortcut.HasValue)
                return shortcut.Value;

            if (fallback.HasValue)
                return fallback.Value;

            throw new InvalidOperationException();
        }
    }

    public ILookupNode<TKey, TValue> Follow(TKey key)
    {
        var direct = shortcut.Follow(key);

        return direct.HasValue ? direct : fallback.Follow(key);
    }
}