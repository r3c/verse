using System.Runtime.InteropServices;

namespace Verse.Schemas.Protobuf
{
#if !__MonoCS__
	[StructLayout(LayoutKind.Explicit, Size = sizeof(ProtobufType) + sizeof(double))]
#endif
	public struct ProtobufValue
	{
		public static readonly ProtobufValue Void;

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
			: this()
        {
            this.Boolean = value;
            this.Type = ProtobufType.Boolean;
        }

        public ProtobufValue(double value)
			: this()
        {
            this.Float64 = value;
            this.Type = ProtobufType.Float64;
        }

        public ProtobufValue(float value)
			: this()
        {
            this.Float32 = value;
            this.Type = ProtobufType.Float32;
        }

        public ProtobufValue(long value)
        	: this()
        {
            this.Signed = value;
            this.Type = ProtobufType.Signed;
        }

        public ProtobufValue(string value)
			: this()
        {
            this.String = value;
            this.Type = ProtobufType.String;
        }

        public ProtobufValue(ulong value)
        	: this()
        {
            this.Type = ProtobufType.Unsigned;
            this.Unsigned = value;
        }
	}
}