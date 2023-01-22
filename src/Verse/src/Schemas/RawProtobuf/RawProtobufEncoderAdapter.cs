using System;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufEncoderAdapter : IEncoderAdapter<RawProtobufValue>
	{
		public Func<bool, RawProtobufValue> FromBoolean =>
			v => new RawProtobufValue(v ? 1 : 0, RawProtobufWireType.VarInt);

		public Func<char, RawProtobufValue> FromCharacter => v =>
			new RawProtobufValue(new string(v, 1), RawProtobufWireType.VarInt);

		public unsafe Func<decimal, RawProtobufValue> FromDecimal => v =>
		{
			var number = (double) v;

			return new RawProtobufValue(*(long*) &number, RawProtobufWireType.Fixed64);
		};

		public unsafe Func<float, RawProtobufValue> FromFloat32 =>
			v => new RawProtobufValue(*(int*) &v, RawProtobufWireType.Fixed32);

		public unsafe Func<double, RawProtobufValue> FromFloat64 => v =>
			new RawProtobufValue(*(long*) &v, RawProtobufWireType.Fixed64);

		public Func<sbyte, RawProtobufValue> FromInteger8S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

		public Func<byte, RawProtobufValue> FromInteger8U => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

		public Func<short, RawProtobufValue> FromInteger16S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

		public Func<ushort, RawProtobufValue> FromInteger16U =>
			v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

		public Func<int, RawProtobufValue> FromInteger32S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

		public Func<uint, RawProtobufValue> FromInteger32U => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

		public Func<long, RawProtobufValue> FromInteger64S => v => new RawProtobufValue(v, RawProtobufWireType.VarInt);

		public unsafe Func<ulong, RawProtobufValue> FromInteger64U =>
			v => new RawProtobufValue(*(long*) v, RawProtobufWireType.VarInt);

		public Func<string, RawProtobufValue> FromString => v => new RawProtobufValue(v, RawProtobufWireType.String);
	}
}
