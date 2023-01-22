namespace Verse;

internal interface ILookupNode<in TKey, out TValue>
{
    bool HasValue { get; }

    TValue Value { get; }

    ILookupNode<TKey, TValue> Follow(TKey key);
}