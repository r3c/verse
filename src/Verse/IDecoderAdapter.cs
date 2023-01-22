namespace Verse;

/// <summary>
/// Decoder adapter provides converters from schema native value type to C# standard types that can be used when
/// declaring value from a <see cref="IDecoderDescriptor{TValue,TEntity}"/> instance that maps to such type.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
public interface IDecoderAdapter<in TNative>
{
    Setter<bool, TNative> ToBoolean { get; }

    Setter<char, TNative> ToCharacter { get; }

    Setter<decimal, TNative> ToDecimal { get; }

    Setter<float, TNative> ToFloat32 { get; }

    Setter<double, TNative> ToFloat64 { get; }

    Setter<sbyte, TNative> ToInteger8S { get; }

    Setter<byte, TNative> ToInteger8U { get; }

    Setter<short, TNative> ToInteger16S { get; }

    Setter<ushort, TNative> ToInteger16U { get; }

    Setter<int, TNative> ToInteger32S { get; }

    Setter<uint, TNative> ToInteger32U { get; }

    Setter<long, TNative> ToInteger64S { get; }

    Setter<ulong, TNative> ToInteger64U { get; }

    Setter<string, TNative> ToString { get; }
}