using System;

namespace Verse.Resolvers;

internal readonly struct TypeResolver
{
    public readonly Type Type;

    public static TypeResolver Create(Type type)
    {
        return new TypeResolver(type);
    }

    private TypeResolver(Type type)
    {
        Type = type;
    }

    /// <Summary>
    /// Check whether type is a generic type with same type definition than type argument `TGeneric`, e.g. when called
    /// with `IEnumerable&lt;object&gt;` as `TGeneric` check that type is any `IEnumerable&lt;T&gt;`.
    /// </Summary>
    public bool HasSameDefinitionThan<TGeneric>()
    {
        var expected = typeof(TGeneric);

        if (!expected.IsGenericType)
            throw new InvalidOperationException("type is not generic");

        return Type.IsGenericType && Type.GetGenericTypeDefinition() == expected.GetGenericTypeDefinition();
    }
}