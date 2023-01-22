using Verse.LookupNodes;

namespace Verse.Lookups;

internal class NameLookup<TValue> : ILookup<char, TValue>
{
    public static readonly ILookup<char, TValue> Empty = new NameLookup<TValue>();

    public ILookupNode<char, TValue> Root => root;

    private readonly HashLookupNode<char, TValue> root;

    public NameLookup()
    {
        root = new HashLookupNode<char, TValue>(k => k);
    }

    public bool ConnectTo(string sequence, TValue value)
    {
        var current = root;

        foreach (var key in sequence)
            current = current.ConnectTo(key);

        if (current.HasValue)
            return false;

        current.HasValue = true;
        current.Value = value;

        return true;
    }
}