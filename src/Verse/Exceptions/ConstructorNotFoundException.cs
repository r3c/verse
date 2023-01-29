using System;

namespace Verse.Exceptions;

public sealed class ConstructorNotFoundException : Exception
{
    public Type Type { get; }

    public ConstructorNotFoundException(Type type) :
        base($"Cannot find parameterless constructor for type {type.FullName}")
    {
        Type = type;
    }
}