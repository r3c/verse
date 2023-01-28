namespace Verse;

/// <summary>
/// Decoder adapter provides converters from schema native value type to C# standard types that can be used when
/// declaring value from a <see cref="IDecoderDescriptor{TValue,TEntity}"/> instance that maps to such type.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
public interface IDecoderAdapter<in TNative>
{
    Setter<bool, TNative> Boolean { get; }

    Setter<char, TNative> Character { get; }

    Setter<decimal, TNative> Decimal { get; }

    Setter<float, TNative> Float32 { get; }

    Setter<double, TNative> Float64 { get; }

    Setter<sbyte, TNative> Integer8S { get; }

    Setter<byte, TNative> Integer8U { get; }

    Setter<short, TNative> Integer16S { get; }

    Setter<ushort, TNative> Integer16U { get; }

    Setter<int, TNative> Integer32S { get; }

    Setter<uint, TNative> Integer32U { get; }

    Setter<long, TNative> Integer64S { get; }

    Setter<ulong, TNative> Integer64U { get; }

    Setter<string, TNative> String { get; }
}