using System;
using System.Collections.Generic;

namespace Verse.LookupNodes;

internal static class HashLookupNode
{
    public const int HashThreshold = 128;
}

internal class HashLookupNode<TKey, TValue>(Func<TKey, int> extractor) : ILookupNode<TKey, TValue>
{
    public bool HasValue { get; set; }
    public TValue Value { get; set; } = default!;

    private static readonly ILookupNode<TKey, TValue> Empty = new HashLookupNode<TKey, TValue>(_ => 0);

    private Dictionary<int, HashLookupNode<TKey, TValue>>? _hashedChildren;

    private HashLookupNode<TKey, TValue>?[]? _indexedChildren;

    public HashLookupNode<TKey, TValue> ConnectTo(TKey key)
    {
        var character = extractor(key);

        if (character < HashLookupNode.HashThreshold)
        {
            _indexedChildren ??= new HashLookupNode<TKey, TValue>[HashLookupNode.HashThreshold];

            var indexedChild = _indexedChildren[character];

            if (indexedChild != null)
                return indexedChild;

            var next = new HashLookupNode<TKey, TValue>(extractor);

            _indexedChildren[character] = next;

            return next;
        }
        else
        {
            _hashedChildren ??= new Dictionary<int, HashLookupNode<TKey, TValue>>();

            if (_hashedChildren.TryGetValue(character, out var next))
                return next;

            next = new HashLookupNode<TKey, TValue>(extractor);

            _hashedChildren[character] = next;

            return next;
        }
    }

    public ILookupNode<TKey, TValue> Follow(TKey key)
    {
        var character = extractor(key);

        if (character < HashLookupNode.HashThreshold)
        {
            var indexedChild = _indexedChildren?[character];

            if (indexedChild != null)
                return indexedChild;
        }
        else
        {
            if (_hashedChildren != null && _hashedChildren.TryGetValue(character, out var next))
                return next;
        }

        return Empty;
    }
}