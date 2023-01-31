using System;
using System.Collections.Generic;
using Verse.Generators;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class ArrayEncodeLinker<TNative> : IEncodeLinker<TNative>
{
    public static readonly ArrayEncodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        var entityType = typeof(TEntity);

        // Try to bind descriptor as an array
        if (entityType.IsArray)
        {
            var elementType = entityType.GetElementType();

            if (elementType is null)
                return false;

            var getter = MethodResolver
                .Create<Func<Func<object, object>>>(() => GetterGenerator.CreateIdentity<object>())
                .SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(elementType))
                .Invoke(new object());

            return TryDescribeAsArray(context, descriptor, elementType, getter);
        }

        // Try to bind descriptor as an instance of IEnumerable<>
        foreach (var interfaceType in entityType.GetInterfaces())
        {
            // Make sure that interface is IEnumerable<T> and store typeof(T)
            var interfaceResolver = TypeResolver.Create(interfaceType);

            if (!interfaceResolver.HasSameDefinitionThan<IEnumerable<object>>(out var arguments))
                continue;

            var elementType = arguments[0];
            var getter = MethodResolver
                .Create<Func<Func<object, object>>>(() => GetterGenerator.CreateIdentity<object>())
                .SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(elementType))
                .Invoke(new object());

            return TryDescribeAsArray(context, descriptor, elementType, getter);
        }

        // Type doesn't seem compatible with array
        return false;
    }

    private static bool TryDescribeAsArray<TEntity>(EncodeContext<TNative> context,
        IEncoderDescriptor<TNative, TEntity> descriptor, Type elementType, object getter)
    {
        if (context.Parents.TryGetValue(elementType, out var recurse))
        {
            MethodResolver
                .Create<Func<IEncoderDescriptor<TNative, TEntity>, Func<TEntity, IEnumerable<object>>,
                    IEncoderDescriptor<TNative, object>,
                    IEncoderDescriptor<TNative, object>>>((d, a, p) => d.IsArray(a, p))
                .SetGenericArguments(elementType)
                .Invoke(descriptor, getter, recurse);

            return true;
        }

        var itemDescriptor = MethodResolver
            .Create<Func<IEncoderDescriptor<TNative, TEntity>, Func<TEntity, IEnumerable<object>>,
                IEncoderDescriptor<TNative, object>>>(
                (d, a) => d.IsArray(a))
            .SetGenericArguments(elementType)
            .Invoke(descriptor, getter);

        return (bool)MethodResolver
            .Create<Func<IEncodeLinker<TNative>, EncodeContext<TNative>, IEncoderDescriptor<TNative, object>, bool>>(
                (l, c, d) => l.TryDescribe(c, d))
            .SetGenericArguments(elementType)
            .Invoke(context.Automatic, context, itemDescriptor);
    }
}