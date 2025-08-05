using System;
using System.Collections.Generic;
using System.Linq;
using Verse.Generators;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class ArrayEncodeLinker<TNative> : IEncodeLinker<TNative>
{
    private static readonly MethodResolver TryDescribeAsArrayMethod =
        MethodResolver
            .Create<Func<EncodeContext<TNative>, IEncoderDescriptor<TNative, object>, Func<object, IEnumerable<object>>,
                bool>>((c, d, g) => TryDescribeAsArray(c, d, g));

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
                .InvokeStatic();

            return (bool)TryDescribeAsArrayMethod
                .SetGenericArguments(entityType, elementType)
                .InvokeStatic(context, descriptor, getter)!;
        }

        // Try to bind descriptor as an instance of `IEnumerable<T>`
        foreach (var interfaceType in entityType.GetInterfaces().Append(entityType))
        {
            // Make sure that interface is `IEnumerable<T>` and store `typeof(T)`
            var interfaceResolver = TypeResolver.Create(interfaceType);

            if (!interfaceResolver.HasSameDefinitionThan<IEnumerable<object>>())
                continue;

            var elementType = interfaceResolver.Type.GetGenericArguments()[0];
            var outputType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var getter = MethodResolver
                .Create<Func<Func<object, object>>>(() => GetterGenerator.CreateIdentity<object>())
                .SetGenericArguments(outputType)
                .InvokeStatic();

            return (bool)TryDescribeAsArrayMethod
                .SetGenericArguments(entityType, elementType)
                .InvokeStatic(context, descriptor, getter)!;
        }

        // Type doesn't seem compatible with array
        return false;
    }

    private static bool TryDescribeAsArray<TEntity, TElement>(EncodeContext<TNative> context,
        IEncoderDescriptor<TNative, TEntity> descriptor, Func<TEntity, IEnumerable<TElement>> getter)
    {
        if (context.Parents.TryGetValue(typeof(TElement), out var recurse))
        {
            descriptor.IsArray(getter, (IEncoderDescriptor<TNative, TElement>)recurse);

            return true;
        }

        var itemDescriptor = descriptor.IsArray(getter);

        return context.Automatic.TryDescribe(context, itemDescriptor);
    }
}