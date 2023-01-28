using System.Runtime.InteropServices;

namespace Verse.Schemas.Protobuf;
#if !__MonoCS__
[StructLayout(LayoutKind.Explicit, Size = sizeof(ProtobufType) + sizeof(double))]
#endif
public struct ProtobufValue
{
    public static readonly ProtobufValue Empty;

#if !__MonoCS__
    [FieldOffset(0)]
#endif
    public readonly ProtobufType Type;

#if !__MonoCS__
    [FieldOffset(sizeof(ProtobufType))]
#endif
    public readonly bool Boolean;

#if !__MonoCS__
    [FieldOffset(sizeof(ProtobufType))]
#endif
    public readonly double Float64;

#if !__MonoCS__
    [FieldOffset(sizeof(ProtobufType))]
#endif
    public readonly float Float32;

#if !__MonoCS__
    [FieldOffset(sizeof(ProtobufType))]
#endif
    public readonly long Signed;

#if !__MonoCS__
    [FieldOffset(sizeof(ProtobufType))]
#endif
    public readonly ulong Unsigned;

#if !__MonoCS__
    [FieldOffset(sizeof(ProtobufType) + 12)]
#endif
    public readonly string String;

    public ProtobufValue(bool value)
    {
        Boolean = value;
        String = string.Empty;
        Type = ProtobufType.Boolean;
    }

    public ProtobufValue(double value)
    {
        Float64 = value;
        String = string.Empty;
        Type = ProtobufType.Float64;
    }

    public ProtobufValue(float value)
    {
        Float32 = value;
        String = string.Empty;
        Type = ProtobufType.Float32;
    }

    public ProtobufValue(long value)
    {
        Signed = value;
        String = string.Empty;
        Type = ProtobufType.Signed;
    }

    public ProtobufValue(string value)
    {
        String = value;
        Type = ProtobufType.String;
    }

    public ProtobufValue(ulong value)
    {
        String = string.Empty;
        Type = ProtobufType.Unsigned;
        Unsigned = value;
    }
}