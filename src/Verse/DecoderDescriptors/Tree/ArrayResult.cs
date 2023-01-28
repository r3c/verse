namespace Verse.DecoderDescriptors.Tree;

internal record struct ArrayResult<TElement>(TElement? Current, ArrayState State)
{
    public static readonly ArrayResult<TElement> EndOfArray = new(default!, ArrayState.EndOfArray);

    public static readonly ArrayResult<TElement> Failure = new(default!, ArrayState.Failure);
}

internal static class ArrayResult
{
    public static ArrayResult<TElement> NextElement<TElement>(TElement entity)
    {
        return new ArrayResult<TElement>(entity, ArrayState.NextElement);
    }
}