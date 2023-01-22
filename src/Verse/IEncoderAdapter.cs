using System;

namespace Verse;

/// <summary>
/// Encoder adapter provides converters from C# standard types to schema native value type that can be used when
/// declaring value from a <see cref="IEncoderDescriptor{TValue,TEntity}"/> instance that maps to such type.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
public interface IEncoderAdapter<out TNative>
{
    Func<bool, TNative> FromBoolean { get; }

    Func<char, TNative> FromCharacter { get; }

    Func<decimal, TNative> FromDecimal { get; }

    Func<float, TNative> FromFloat32 { get; }

    Func<double, TNative> FromFloat64 { get; }

    Func<sbyte, TNative> FromInteger8S { get; }

    Func<byte, TNative> FromInteger8U { get; }

    Func<short, TNative> FromInteger16S { get; }

    Func<ushort, TNative> FromInteger16U { get; }

    Func<int, TNative> FromInteger32S { get; }

    Func<uint, TNative> FromInteger32U { get; }

    Func<long, TNative> FromInteger64S { get; }

    Func<ulong, TNative> FromInteger64U { get; }

    Func<string, TNative> FromString { get; }
}