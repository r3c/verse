using System.Globalization;
using Verse.LookupNodes;

namespace Verse.Lookups;

internal class IndexOrNameLookup<TValue> : ILookup<int, TValue>
{
    public ILookupNode<int, TValue> Root => _dualRoot;

    private readonly DualLookupNode<int, TValue> _dualRoot;
    private readonly HashLookupNode<int, TValue> _indexRoot;
    private readonly HashLookupNode<int, TValue> _nameRoot;

    public IndexOrNameLookup()
    {
        var index = new HashLookupNode<int, TValue>(k => k);
        var name = new HashLookupNode<int, TValue>(k => k);

        _dualRoot = new DualLookupNode<int, TValue>(index, name);
        _indexRoot = index;
        _nameRoot = name;
    }

    public bool ConnectTo(string sequence, TValue value)
    {
        if (int.TryParse(sequence, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            var indexNode = _indexRoot.ConnectTo(index);

            if (indexNode.HasValue)
                return false;

            indexNode.HasValue = true;
            indexNode.Value = value;
        }

        var current = _nameRoot;

        foreach (var key in sequence)
            current = current.ConnectTo(key);

        if (current.HasValue)
            return false;

        current.HasValue = true;
        current.Value = value;

        return true;
    }
}