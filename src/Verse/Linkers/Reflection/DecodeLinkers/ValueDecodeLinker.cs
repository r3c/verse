using System;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal class ValueDecodeLinker<TNative> : IDecodeLinker<TNative>
{
    public static readonly ValueDecodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        // Try linking using using underlying type if entity is nullable
        if (TypeResolver.Create(typeof(TEntity)).HasSameDefinitionThan<int?>(out var arguments))
        {
            return (bool)MethodResolver
                .Create<Func<DecodeContext<TNative>, IDecoderDescriptor<TNative, int?>, bool>>((c, d) =>
                    TryDescribeAsNullableValue(c, d))
                .SetGenericArguments(arguments[0])
                .Invoke(this, context, descriptor);
        }

        // Otherwise link as non-nullable value
        return TryDescribeAsValue(context, descriptor);
    }

    private static bool TryDescribeAsNullableValue<TEntity>(DecodeContext<TNative> context,
        IDecoderDescriptor<TNative, TEntity?> descriptor)
        where TEntity : struct
    {
        // TODO: move `AdapterResolver.TryGetDecoderConverter` as private method in this class
        if (!AdapterResolver.TryGetDecoderConverter<TNative, TEntity>(context.Format.To, out var converter))
            return false;

        // Assume that default value is equivalent to null entity
        descriptor.IsValue(source => Equals(context.Format.DefaultValue, source) ? default(TEntity?) : converter(source));

        return true;
    }

    private static bool TryDescribeAsValue<TEntity>(DecodeContext<TNative> context,
        IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        // TODO: move `AdapterResolver.TryGetDecoderConverter` as private method in this class
        if (!AdapterResolver.TryGetDecoderConverter<TNative, TEntity>(context.Format.To, out var converter))
            return false;

        descriptor.IsValue(converter);

        return true;
    }
}