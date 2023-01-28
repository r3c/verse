using System;

namespace Verse;

/// <summary>
/// Encoder adapter provides converters from C# standard types to schema native value type that can be used when
/// declaring value from a <see cref="IEncoderDescriptor{TValue,TEntity}"/> instance that maps to such type.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
public interface IEncoderAdapter<out TNative>
{
    Func<bool, TNative> Boolean { get; }

    Func<char, TNative> Character { get; }

    Func<decimal, TNative> Decimal { get; }

    Func<float, TNative> Float32 { get; }

    Func<double, TNative> Float64 { get; }

    Func<sbyte, TNative> Integer8S { get; }

    Func<byte, TNative> Integer8U { get; }

    Func<short, TNative> Integer16S { get; }

    Func<ushort, TNative> Integer16U { get; }

    Func<int, TNative> Integer32S { get; }

    Func<uint, TNative> Integer32U { get; }

    Func<long, TNative> Integer64S { get; }

    Func<ulong, TNative> Integer64U { get; }

    Func<string, TNative> String { get; }
}