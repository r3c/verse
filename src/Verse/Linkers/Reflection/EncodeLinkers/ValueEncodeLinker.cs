using System;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class ValueEncodeLinker<TNative> : IEncodeLinker<TNative>
{
    public static readonly ValueEncodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        // Try linking using using underlying type if entity is nullable
        if (TypeResolver.Create(typeof(TEntity)).HasSameDefinitionThan<int?>(out var arguments))
        {
            return (bool)MethodResolver
                .Create<Func<EncodeContext<TNative>, IEncoderDescriptor<TNative, int?>, bool>>((c, d) =>
                    TryDescribeAsNullableValue(c, d))
                .SetGenericArguments(arguments[0])
                .Invoke(this, context, descriptor);
        }

        // Otherwise link as non-nullable value
        return TryDescribeAsValue(context, descriptor);
    }

    private static bool TryDescribeAsNullableValue<TEntity>(EncodeContext<TNative> context,
        IEncoderDescriptor<TNative, TEntity?> descriptor)
        where TEntity : struct
    {
        // TODO: move `AdapterResolver.TryGetDecoderConverter` as private method in this class
        if (!AdapterResolver.TryGetEncoderConverter<TNative, TEntity>(context.Format.From, out var converter))
            return false;

        // Assume that null entity is equivalent to default value
        descriptor.IsValue(source => source.HasValue ? converter(source.Value) : context.Format.DefaultValue);

        return true;
    }

    private static bool TryDescribeAsValue<TEntity>(EncodeContext<TNative> context,
        IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        // TODO: move `AdapterResolver.TryGetEncoderConverter` as private method in this class
        if (!AdapterResolver.TryGetEncoderConverter<TNative, TEntity>(context.Format.From, out var converter))
            return false;

        descriptor.IsValue(converter);

        return true;
    }
}