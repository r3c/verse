
namespace Verse.Schemas.RawProtobuf
{
	public readonly struct RawProtobufValue
	{
		private static readonly RawProtobufEncoderConverter ConverterFrom = new RawProtobufEncoderConverter();
		private static readonly RawProtobufDecoderConverter ConverterTo = new RawProtobufDecoderConverter();

		public readonly long Number;

		public readonly RawProtobufStorage Storage;

		public readonly string String;

		public RawProtobufValue(long number, RawProtobufStorage storage)
		{
			this.Number = number;
			this.Storage = storage;
			this.String = default;
		}

		public RawProtobufValue(string value, RawProtobufStorage storage)
		{
			this.Number = default;
			this.Storage = storage;
			this.String = value;
		}

		public static RawProtobufValue FromLong(long value)
		{
			return RawProtobufValue.ConverterFrom.Get<long>()(value);
		}

		public static RawProtobufValue FromFloat(float value)
		{
			return RawProtobufValue.ConverterFrom.Get<float>()(value);
		}

		public static RawProtobufValue FromDouble(double value)
		{
			return RawProtobufValue.ConverterFrom.Get<double>()(value);
		}

		public static RawProtobufValue FromString(string value)
		{
			return RawProtobufValue.ConverterFrom.Get<string>()(value);
		}

		public static long ToLong(RawProtobufValue value)
		{
			return RawProtobufValue.ConverterTo.Get<long>()(value);
		}

		public static double ToDouble(RawProtobufValue value)
		{
			return RawProtobufValue.ConverterTo.Get<double>()(value);
		}

		public static float ToFloat(RawProtobufValue value)
		{
			return RawProtobufValue.ConverterTo.Get<float>()(value);
		}

		public static string ToString(RawProtobufValue value)
		{
			return RawProtobufValue.ConverterTo.Get<string>()(value);
		}
	}
}
