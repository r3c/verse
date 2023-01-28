using System;
using System.Collections.Generic;

namespace Verse.Resolvers;

/// <summary>
/// This resolver is used to retrieve appropriate converter for a given value type and entity type, if available. It
/// is used within <see cref="Linker"/> to trigger "HasValue" linking on current entity type whenever possible.
/// </summary>
internal static class AdapterResolver
{
    private static readonly Dictionary<Type, PropertyResolver> ForDecoder = new()
    {
        {
            typeof(bool),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<bool, object>>>(a => a.Boolean)
        },
        {
            typeof(char),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<char, object>>>(a => a.Character)
        },
        {
            typeof(decimal),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<decimal, object>>>(a => a.Decimal)
        },
        {
            typeof(float),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<float, object>>>(a => a.Float32)
        },
        {
            typeof(double),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<double, object>>>(a => a.Float64)
        },
        {
            typeof(sbyte),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<sbyte, object>>>(a => a.Integer8S)
        },
        {
            typeof(byte),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<byte, object>>>(a => a.Integer8U)
        },
        {
            typeof(short),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<short, object>>>(a => a.Integer16S)
        },
        {
            typeof(ushort),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<ushort, object>>>(a => a.Integer16U)
        },
        {
            typeof(int),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<int, object>>>(a => a.Integer32S)
        },
        {
            typeof(uint),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<uint, object>>>(a => a.Integer32U)
        },
        {
            typeof(long),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<long, object>>>(a => a.Integer64S)
        },
        {
            typeof(ulong),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<ulong, object>>>(a => a.Integer64U)
        },
        {
            typeof(string),
            PropertyResolver.Create<Func<IDecoderAdapter<object>, Setter<string, object>>>(a => a.String)
        }
    };

    private static readonly Dictionary<Type, PropertyResolver> ForEncoder = new()
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

    public static bool TryGetDecoderConverter<TNative, TEntity>(IDecoderAdapter<TNative> adapter,
        out Setter<TEntity, TNative> setter)
    {
        if (!ForDecoder.TryGetValue(typeof(TEntity), out var generator))
        {
            setter = default!;

            return false;
        }

        var untyped = generator.SetCallerGenericArguments(typeof(TNative)).GetGetter(adapter);

        setter = (Setter<TEntity, TNative>) untyped!;

        return true;
    }

    public static bool TryGetEncoderConverter<TNative, TEntity>(IEncoderAdapter<TNative> adapter,
        out Func<TEntity, TNative> getter)
    {
        if (!ForEncoder.TryGetValue(typeof(TEntity), out var generator))
        {
            getter = default!;

            return false;
        }

        var untyped = generator.SetCallerGenericArguments(typeof(TNative)).GetGetter(adapter);

        getter = (Func<TEntity, TNative>) untyped!;

        return true;
    }
}