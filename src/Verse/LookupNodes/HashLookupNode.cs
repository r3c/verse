using System;
using System.Collections.Generic;

namespace Verse.LookupNodes;

internal static class HashLookupNode
{
    public const int HashThreshold = 128;
}

internal class HashLookupNode<TKey, TValue> : ILookupNode<TKey, TValue>
{
    public bool HasValue { get; set; }
    public TValue Value { get; set; }

    private static readonly ILookupNode<TKey, TValue> Empty = new HashLookupNode<TKey, TValue>(k => default);

    private readonly Func<TKey, int> _extractor;

    private Dictionary<int, HashLookupNode<TKey, TValue>> _hashedChildren;

    private HashLookupNode<TKey, TValue>[] _indexedChildren;

    public HashLookupNode(Func<TKey, int> extractor)
    {
        _extractor = extractor;
    }

    public HashLookupNode<TKey, TValue> ConnectTo(TKey key)
    {
        var character = _extractor(key);

        if (character < HashLookupNode.HashThreshold)
        {
            if (_indexedChildren == null)
                _indexedChildren = new HashLookupNode<TKey, TValue>[HashLookupNode.HashThreshold];

            if (_indexedChildren[character] != null)
                return _indexedChildren[character];

            var next = new HashLookupNode<TKey, TValue>(_extractor);

            _indexedChildren[character] = next;

            return next;
        }
        else
        {
            if (_hashedChildren == null)
                _hashedChildren = new Dictionary<int, HashLookupNode<TKey, TValue>>();

            if (_hashedChildren.TryGetValue(character, out var next))
                return next;

            next = new HashLookupNode<TKey, TValue>(_extractor);

            _hashedChildren[character] = next;

            return next;
        }
    }

    public ILookupNode<TKey, TValue> Follow(TKey key)
    {
        var character = _extractor(key);

        if (character < HashLookupNode.HashThreshold)
        {
            if (_indexedChildren != null && _indexedChildren[character] != null)
                return _indexedChildren[character];
        }
        else
        {
            if (_hashedChildren != null && _hashedChildren.TryGetValue(character, out var next))
                return next;
        }

        return Empty;
    }
}