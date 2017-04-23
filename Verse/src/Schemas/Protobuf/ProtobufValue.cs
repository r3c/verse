using System.Runtime.InteropServices;
namespace Verse.Schemas.Protobuf
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ProtobufValue
	{
		public static readonly ProtobufValue Void = new ProtobufValue();

		#region Attributes

        [FieldOffset(4)]
        public readonly bool Boolean;

        [FieldOffset(4)]
        public readonly double Float64;

        [FieldOffset(4)]
        public readonly float Float32;

        [FieldOffset(4)]
        public readonly long Signed;

        [FieldOffset(12)]
        public readonly string String;

        [FieldOffset(0)]
        public readonly ProtobufType Type;

        [FieldOffset(4)]
        public readonly ulong Unsigned;

		#endregion

		#region Constructor

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

		#endregion
	}
}