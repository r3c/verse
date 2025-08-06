using System;
using System.Collections.Generic;
using System.Reflection;
using Verse.Generators;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal static class ArrayDecodeLinker
{
    private static readonly ConstructorResolver HashSetConstructorResolver =
        ConstructorResolver.Create<Func<IEnumerable<Any>, HashSet<Any>>>(e => new HashSet<Any>(e));

    private static readonly ConstructorResolver ListConstructorResolver =
        ConstructorResolver.Create<Func<IEnumerable<Any>, List<Any>>>(e => new List<Any>(e));

    public static readonly IReadOnlyList<(Type, ConstructorResolver)> KnownInterfaces = new[]
    {
        (typeof(ISet<Any>), HashSetConstructorResolver),
#if NET6_0_OR_GREATER
        (typeof(IReadOnlySet<Any>), HashSetConstructorResolver),
#endif
        (typeof(IList<Any>), ListConstructorResolver),
        (typeof(IReadOnlyList<Any>), ListConstructorResolver),
        (typeof(ICollection<Any>), ListConstructorResolver),
        (typeof(IReadOnlyCollection<Any>), ListConstructorResolver),
        (typeof(IEnumerable<Any>), ListConstructorResolver)
    };
}

internal class ArrayDecodeLinker<TNative> : IDecodeLinker<TNative>
{
    private static readonly MethodResolver ArrayDecodeLinkerTryDescribeAsArray =
        MethodResolver
            .Create<Func<DecodeContext<TNative>, IDecoderDescriptor<TNative, Any>, Func<IEnumerable<Any>, Any>, bool>>((
                c, d, s) => TryDescribeAsArray(c, d, s));

    public static readonly ArrayDecodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        var entityType = typeof(TEntity);

        // Try to bind descriptor as an array
        if (entityType.IsArray)
        {
            var elementType = entityType.GetElementType();

            if (elementType is null)
                return false;

            var converter = MethodResolver
                .Create<Func<Func<IEnumerable<Any>, Any[]>>>(() => ConverterGenerator.CreateFromEnumerable<Any>())
                .SetGenericArguments(elementType)
                .InvokeStatic();

            return (bool)ArrayDecodeLinkerTryDescribeAsArray
                .SetGenericArguments(entityType, elementType)
                .InvokeStatic(context, descriptor, converter)!;
        }

        // Try to bind descriptor as a known list-like interface (e.g. `IReadOnlyList<T>` or `ICollection<T>`)
        if (entityType is { IsInterface: true, IsGenericType: true })
        {
            var interfaceResolver = TypeResolver.Create(entityType);

            foreach (var (interfaceType, constructorResolver) in ArrayDecodeLinker.KnownInterfaces)
            {
                // Check if current entity is a `ISomething<T>` where `ISomething` is a known list-like interface
                var hasSameDefinition = (bool)MethodResolver
                    .Create<Func<TypeResolver, bool>>(r => r.HasSameDefinitionThan<object>())
                    .SetGenericArguments(interfaceType)
                    .InvokeInstance(interfaceResolver)!;

                if (!hasSameDefinition)
                    continue;

                // Resolve constructor from `IEnumerable<T>` input
                var elementType = interfaceResolver.Type.GetGenericArguments()[0];
                var inputType = typeof(IEnumerable<>).MakeGenericType(elementType);
                var constructor = constructorResolver.SetTypeGenericArguments(elementType).Constructor;

                var converter = MethodResolver
                    .Create<Func<ConstructorInfo, Func<Any, Any>>>(c =>
                        ConverterGenerator.CreateFromConstructor<Any, Any>(c))
                    .SetGenericArguments(constructor.DeclaringType!, inputType)
                    .InvokeStatic(constructor);

                return (bool)ArrayDecodeLinkerTryDescribeAsArray
                    .SetGenericArguments(entityType, elementType)
                    .InvokeStatic(context, descriptor, converter)!;
            }
        }

        // Try to bind descriptor as a custom implementation of `IEnumerable<T>`
        foreach (var interfaceType in entityType.GetInterfaces())
        {
            // Make sure that interface is `IEnumerable<T>` and store `typeof(T)`
            var interfaceResolver = TypeResolver.Create(interfaceType);

            if (!interfaceResolver.HasSameDefinitionThan<IEnumerable<object>>())
                continue;

            var elementType = interfaceResolver.Type.GetGenericArguments()[0];

            // Search constructor compatible with `IEnumerable<T>`
            foreach (var constructor in entityType.GetConstructors())
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length != 1)
                    continue;

                var parameterType = parameters[0].ParameterType;
                var parameterResolver = TypeResolver.Create(parameterType);

                if (!parameterResolver.HasSameDefinitionThan<IEnumerable<object>>() ||
                    interfaceResolver.Type.GetGenericArguments()[0] != elementType)
                    continue;

                var converter = MethodResolver
                    .Create<Func<ConstructorInfo, Func<Any, Any>>>(c =>
                        ConverterGenerator.CreateFromConstructor<Any, Any>(c))
                    .SetGenericArguments(entityType, parameterType)
                    .InvokeStatic(constructor);

                return (bool)ArrayDecodeLinkerTryDescribeAsArray
                    .SetGenericArguments(entityType, elementType)
                    .InvokeStatic(context, descriptor, converter)!;
            }
        }

        // Type doesn't seem compatible with array
        return false;
    }

    private static bool TryDescribeAsArray<TEntity, TElement>(DecodeContext<TNative> context,
        IDecoderDescriptor<TNative, TEntity> descriptor, Func<IEnumerable<TElement>, TEntity> converter)
    {
        if (context.Parents.TryGetValue(typeof(TElement), out var recurse))
        {
            descriptor.IsArray(converter, (IDecoderDescriptor<TNative, TElement>)recurse);

            return true;
        }

        var elementDescriptor = descriptor.IsArray(converter);

        return context.Automatic.TryDescribe(context, elementDescriptor);
    }
}