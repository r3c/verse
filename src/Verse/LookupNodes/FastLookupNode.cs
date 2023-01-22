namespace Verse.LookupNodes;

internal class FastLookupNode<TKey, TValue> : ILookupNode<TKey, TValue>
{
    public bool HasValue => shortcut.HasValue || fallback.HasValue;
    public TValue Value => shortcut.HasValue ? shortcut.Value : fallback.Value;

    private readonly ILookupNode<TKey, TValue> fallback;
    private readonly ILookupNode<TKey, TValue> shortcut;

    public FastLookupNode(ILookupNode<TKey, TValue> shortcut, ILookupNode<TKey, TValue> fallback)
    {
        this.fallback = fallback;
        this.shortcut = shortcut;
    }

    public ILookupNode<TKey, TValue> Follow(TKey key)
    {
        var direct = shortcut.Follow(key);

        return direct.HasValue ? direct : fallback.Follow(key);
    }
}