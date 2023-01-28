using Verse.LookupNodes;

namespace Verse.Lookups;

internal class NameLookup<TValue> : ILookup<char, TValue>
{
    public ILookupNode<char, TValue> Root => _root;

    private readonly HashLookupNode<char, TValue> _root;

    public NameLookup()
    {
        _root = new HashLookupNode<char, TValue>(k => k);
    }

    public bool ConnectTo(string sequence, TValue value)
    {
        var current = _root;

        foreach (var key in sequence)
            current = current.ConnectTo(key);

        if (current.HasValue)
            return false;

        current.HasValue = true;
        current.Value = value;

        return true;
    }
}