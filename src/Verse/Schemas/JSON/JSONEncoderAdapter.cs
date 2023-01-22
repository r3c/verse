using System;

namespace Verse.Schemas.JSON
{
	internal class JSONEncoderAdapter : IEncoderAdapter<JSONValue>
	{
		public Func<bool, JSONValue> FromBoolean => JSONValue.FromBoolean;

		public Func<char, JSONValue> FromCharacter => v => JSONValue.FromString(new string(v, 1));

		public Func<decimal, JSONValue> FromDecimal => v => JSONValue.FromNumber((double) v);

		public Func<float, JSONValue> FromFloat32 => v => JSONValue.FromNumber(v);

		public Func<double, JSONValue> FromFloat64 => JSONValue.FromNumber;

		public Func<sbyte, JSONValue> FromInteger8S => v => JSONValue.FromNumber(v);

		public Func<byte, JSONValue> FromInteger8U => v => JSONValue.FromNumber(v);

		public Func<short, JSONValue> FromInteger16S => v => JSONValue.FromNumber(v);

		public Func<ushort, JSONValue> FromInteger16U => v => JSONValue.FromNumber(v);

		public Func<int, JSONValue> FromInteger32S => v => JSONValue.FromNumber(v);

		public Func<uint, JSONValue> FromInteger32U => v => JSONValue.FromNumber(v);

		public Func<long, JSONValue> FromInteger64S => v => JSONValue.FromNumber(v);

		public Func<ulong, JSONValue> FromInteger64U => v => JSONValue.FromNumber(v);

		public Func<string, JSONValue> FromString => JSONValue.FromString;
	}
}
