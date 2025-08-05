using System;
using System.Collections.Generic;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal static class ValueEncodeLinker
{
    internal static bool TryGetConverter<TNative, TEntity>(IEncoderAdapter<TNative> adapter,
        out Func<TEntity, TNative> converter)
    {
        if (!AdapterConverters.TryGetValue(typeof(TEntity), out var resolver))
        {
            converter = null!;

            return false;
        }

        var untyped = resolver.SetCallerGenericArguments(typeof(TNative)).GetGetter(adapter);

        converter = (Func<TEntity, TNative>)untyped!;

        return true;
    }

    private static readonly Dictionary<Type, PropertyResolver> AdapterConverters = new()
    {
        {
            typeof(bool),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<bool, object>>>(a => a.Boolean)
        },
        {
            typeof(char),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<char, object>>>(a => a.Character)
        },
        {
            typeof(decimal),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<decimal, object>>>(a => a.Decimal)
        },
        {
            typeof(float),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<float, object>>>(a => a.Float32)
        },
        {
            typeof(double),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<double, object>>>(a => a.Float64)
        },
        {
            typeof(sbyte),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<sbyte, object>>>(a => a.Integer8S)
        },
        {
            typeof(byte),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<byte, object>>>(a => a.Integer8U)
        },
        {
            typeof(short),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<short, object>>>(a => a.Integer16S)
        },
        {
            typeof(ushort),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<ushort, object>>>(a => a.Integer16U)
        },
        {
            typeof(int),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<int, object>>>(a => a.Integer32S)
        },
        {
            typeof(uint),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<uint, object>>>(a => a.Integer32U)
        },
        {
            typeof(long),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<long, object>>>(a => a.Integer64S)
        },
        {
            typeof(ulong),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<ulong, object>>>(a => a.Integer64U)
        },
        {
            typeof(string),
            PropertyResolver.Create<Func<IEncoderAdapter<object>, Func<string, object>>>(a => a.String)
        }
    };
}

internal class ValueEncodeLinker<TNative> : IEncodeLinker<TNative>
{
    public static readonly ValueEncodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        // Try linking using using underlying type if entity is nullable
        var typeResolver = TypeResolver.Create(typeof(TEntity));

        if (typeResolver.HasSameDefinitionThan<int?>())
        {
            return (bool)MethodResolver
                .Create<Func<EncodeContext<TNative>, IEncoderDescriptor<TNative, int?>, bool>>((c, d) =>
                    TryDescribeAsNullableValue(c, d))
                .SetGenericArguments(typeResolver.Type.GetGenericArguments())
                .InvokeStatic(context, descriptor)!;
        }

        // Otherwise link as non-nullable value
        return TryDescribeAsValue(context, descriptor);
    }

    private static bool TryDescribeAsNullableValue<TEntity>(EncodeContext<TNative> context,
        IEncoderDescriptor<TNative, TEntity?> descriptor)
        where TEntity : struct
    {
        if (!ValueEncodeLinker.TryGetConverter<TNative, TEntity>(context.Format.From, out var converter))
            return false;

        // Assume that null entity is equivalent to default value
        descriptor.IsValue(source => source.HasValue ? converter(source.Value) : context.Format.DefaultValue);

        return true;
    }

    private static bool TryDescribeAsValue<TEntity>(EncodeContext<TNative> context,
        IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        if (!ValueEncodeLinker.TryGetConverter<TNative, TEntity>(context.Format.From, out var converter))
            return false;

        descriptor.IsValue(converter);

        return true;
    }
}