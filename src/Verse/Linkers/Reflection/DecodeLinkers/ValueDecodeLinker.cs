using System;
using System.Collections.Generic;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal static class ValueDecodeLinker
{
    internal static bool TryGetConverter<TNative, TEntity>(IDecoderAdapter<TNative> adapter,
        out Func<TNative, TEntity> converter)
    {
        if (!AdapterConverters.TryGetValue(typeof(TEntity), out var resolver))
        {
            converter = null!;

            return false;
        }

        var untyped = resolver.SetCallerGenericArguments(typeof(TNative)).GetGetter(adapter);

        converter = (Func<TNative, TEntity>)untyped!;

        return true;
    }

    private static readonly Dictionary<Type, PropertyResolver> AdapterConverters = new()
    {
        {
            typeof(bool),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, bool>>>(a => a.Boolean)
        },
        {
            typeof(char),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, char>>>(a => a.Character)
        },
        {
            typeof(decimal),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, decimal>>>(a => a.Decimal)
        },
        {
            typeof(float),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, float>>>(a => a.Float32)
        },
        {
            typeof(double),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, double>>>(a => a.Float64)
        },
        {
            typeof(sbyte),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, sbyte>>>(a => a.Integer8S)
        },
        {
            typeof(byte),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, byte>>>(a => a.Integer8U)
        },
        {
            typeof(short),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, short>>>(a => a.Integer16S)
        },
        {
            typeof(ushort),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, ushort>>>(a => a.Integer16U)
        },
        {
            typeof(int),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, int>>>(a => a.Integer32S)
        },
        {
            typeof(uint),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, uint>>>(a => a.Integer32U)
        },
        {
            typeof(long),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, long>>>(a => a.Integer64S)
        },
        {
            typeof(ulong),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, ulong>>>(a => a.Integer64U)
        },
        {
            typeof(string),
            PropertyResolver.Create<Func<IDecoderAdapter<Any>, Func<Any, string>>>(a => a.String)
        }
    };
}

internal class ValueDecodeLinker<TNative> : IDecodeLinker<TNative>
{
    public static readonly ValueDecodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        // Try linking using using underlying type if entity is nullable
        var typeResolver = TypeResolver.Create(typeof(TEntity));

        if (typeResolver.HasSameDefinitionThan<int?>())
        {
            return (bool)MethodResolver
                .Create<Func<DecodeContext<TNative>, IDecoderDescriptor<TNative, int?>, bool>>((c, d) =>
                    TryDescribeAsNullableValue(c, d))
                .SetGenericArguments(typeResolver.Type.GetGenericArguments())
                .InvokeStatic(context, descriptor)!;
        }

        // Otherwise link as non-nullable value
        return TryDescribeAsValue(context, descriptor);
    }

    private static bool TryDescribeAsNullableValue<TEntity>(DecodeContext<TNative> context,
        IDecoderDescriptor<TNative, TEntity?> descriptor)
        where TEntity : struct
    {
        if (!ValueDecodeLinker.TryGetConverter<TNative, TEntity>(context.Format.To, out var converter))
            return false;

        // Assume that default value is equivalent to null entity
        descriptor.IsValue(source => Equals(context.Format.DefaultValue, source) ? null : converter(source));

        return true;
    }

    private static bool TryDescribeAsValue<TEntity>(DecodeContext<TNative> context,
        IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        if (!ValueDecodeLinker.TryGetConverter<TNative, TEntity>(context.Format.To, out var converter))
            return false;

        descriptor.IsValue(converter);

        return true;
    }
}