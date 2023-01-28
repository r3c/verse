using System;

namespace Verse.LookupNodes;

internal class FastLookupNode<TKey, TValue> : ILookupNode<TKey, TValue>
{
    public bool HasValue => _shortcut.HasValue || _fallback.HasValue;

    public TValue Value
    {
        get
        {
            if (_shortcut.HasValue)
                return _shortcut.Value;

            if (_fallback.HasValue)
                return _fallback.Value;

            throw new InvalidOperationException();
        }
    }

    private readonly ILookupNode<TKey, TValue> _fallback;
    private readonly ILookupNode<TKey, TValue> _shortcut;

    public FastLookupNode(ILookupNode<TKey, TValue> shortcut, ILookupNode<TKey, TValue> fallback)
    {
        _fallback = fallback;
        _shortcut = shortcut;
    }

    public ILookupNode<TKey, TValue> Follow(TKey key)
    {
        var direct = _shortcut.Follow(key);

        return direct.HasValue ? direct : _fallback.Follow(key);
    }
}