using System;

namespace Verse.Exceptions;

public sealed class ConstructorNotFoundException(Type type)
    : Exception($"Cannot find parameterless constructor for type {type.FullName}")
{
    public Type Type { get; } = type;
}