using System;
using System.Collections.Generic;
using System.Reflection;
using Verse.Generators;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal class ArrayDecodeLinker<TNative> : IDecodeLinker<TNative>
{
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
                .Create<Func<Func<IEnumerable<object>, object[]>>>(() =>
                    ConverterGenerator.CreateFromEnumerable<object>())
                .SetGenericArguments(elementType)
                .Invoke(new object());

            return TryDescribeAsArray(context, descriptor, elementType, converter);
        }

        // Try to bind descriptor as a known interface compatible with IEnumerable<>
        if (entityType.IsInterface)
        {
            var interfaceResolver = TypeResolver.Create(entityType);

            if (interfaceResolver.HasSameDefinitionThan<IEnumerable<object>>(out var interfaceTypes))
            {
                var elementType = interfaceTypes[0];

                var constructor = ConstructorResolver
                    .Create<Func<IEnumerable<object>, List<object>>>(e => new List<object>(e))
                    .SetTypeGenericArguments(elementType)
                    .Constructor;

                var converter = MethodResolver
                    .Create<Func<ConstructorInfo, Func<object, object>>>(c =>
                        ConverterGenerator.CreateFromConstructor<object, object>(c))
                    .SetGenericArguments(constructor.DeclaringType!, entityType)
                    .Invoke(new object(), constructor);

                return TryDescribeAsArray(context, descriptor, elementType, converter);
            }
        }

        // Try to bind descriptor as an instance of IEnumerable<>
        foreach (var interfaceType in entityType.GetInterfaces())
        {
            // Make sure that interface is IEnumerable<T> and store typeof(T)
            var interfaceResolver = TypeResolver.Create(interfaceType);

            if (!interfaceResolver.HasSameDefinitionThan<IEnumerable<object>>(out var interfaceArgumentTypes))
                continue;

            var elementType = interfaceArgumentTypes[0];

            // Search constructor compatible with IEnumerable<>
            foreach (var constructor in entityType.GetConstructors())
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length != 1)
                    continue;

                var parameterType = parameters[0].ParameterType;
                var parameterResolver = TypeResolver.Create(parameterType);

                if (!parameterResolver.HasSameDefinitionThan<IEnumerable<object>>(out var parameterTypes) ||
                    parameterTypes[0] != elementType)
                    continue;

                var converter = MethodResolver
                    .Create<Func<ConstructorInfo, Func<object, object>>>(c =>
                        ConverterGenerator.CreateFromConstructor<object, object>(c))
                    .SetGenericArguments(entityType, parameterType)
                    .Invoke(new object(), constructor);

                return TryDescribeAsArray(context, descriptor, elementType, converter);
            }
        }

        // Type doesn't seem compatible with array
        return false;
    }

    private static bool TryDescribeAsArray<TEntity>(DecodeContext<TNative> context,
        IDecoderDescriptor<TNative, TEntity> descriptor, Type elementType, object converter)
    {
        if (context.Parents.TryGetValue(elementType, out var recurse))
        {
            MethodResolver
                .Create<Func<IDecoderDescriptor<TNative, TEntity>, Func<IEnumerable<object>, TEntity>,
                    IDecoderDescriptor<TNative, object>, IDecoderDescriptor<TNative, object>>>((d, c, p) =>
                    d.IsArray(c, p))
                .SetGenericArguments(elementType)
                .Invoke(descriptor, converter, recurse);

            return true;
        }

        var elementDescriptor = MethodResolver
            .Create<Func<IDecoderDescriptor<TNative, TEntity>, Func<IEnumerable<object>, TEntity>,
                IDecoderDescriptor<TNative, object>>>(
                (d, c) => d.IsArray(c))
            .SetGenericArguments(elementType)
            .Invoke(descriptor, converter);

        return (bool)MethodResolver
            .Create<Func<IDecodeLinker<TNative>, DecodeContext<TNative>, IDecoderDescriptor<TNative, object>, bool>>(
                (l, c, d) => l.TryDescribe(c, d))
            .SetGenericArguments(elementType)
            .Invoke(context.Automatic, context, elementDescriptor);
    }
}