
namespace Verse.Schemas.RawProtobuf
{
	public readonly struct RawProtobufValue
	{
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

	    private static readonly RawProtobufEncoderConverter converterFrom = new RawProtobufEncoderConverter();
	    private static readonly RawProtobufDecoderConverter converterTo = new RawProtobufDecoderConverter();

	    public static RawProtobufValue FromLong(long value)
	    {
	        return converterFrom.Get<long>()(value);
	    }
	    public static RawProtobufValue FromFloat(float value)
	    {
	        return converterFrom.Get<float>()(value);
	    }
	    public static RawProtobufValue FromDouble(double value)
	    {
	        return converterFrom.Get<double>()(value);
	    }
	    public static RawProtobufValue FromString(string value)
	    {
	        return converterFrom.Get<string>()(value);
	    }

	    public static long ToLong(RawProtobufValue value)
	    {
	        return converterTo.Get<long>()(value);
	    }

	    public static double ToDouble(RawProtobufValue value)
	    {
	        return converterTo.Get<double>()(value);
	    }

	    public static float ToFloat(RawProtobufValue value)
	    {
	        return converterTo.Get<float>()(value);
	    }

	    public static string ToString(RawProtobufValue value)
	    {
	        return converterTo.Get<string>()(value);
	    }
    }
}