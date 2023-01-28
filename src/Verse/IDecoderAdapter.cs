using System;

namespace Verse;

/// <summary>
/// Decoder adapter provides converters from schema native value type to C# standard types that can be used when
/// declaring value from a <see cref="IDecoderDescriptor{TValue,TEntity}"/> instance that maps to such type.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
public interface IDecoderAdapter<in TNative>
{
    Func<TNative, bool> Boolean { get; }

    Func<TNative, char> Character { get; }

    Func<TNative, decimal> Decimal { get; }

    Func<TNative, float> Float32 { get; }

    Func<TNative, double> Float64 { get; }

    Func<TNative, sbyte> Integer8S { get; }

    Func<TNative, byte> Integer8U { get; }

    Func<TNative, short> Integer16S { get; }

    Func<TNative, ushort> Integer16U { get; }

    Func<TNative, int> Integer32S { get; }

    Func<TNative, uint> Integer32U { get; }

    Func<TNative, long> Integer64S { get; }

    Func<TNative, ulong> Integer64U { get; }

    Func<TNative, string> String { get; }
}