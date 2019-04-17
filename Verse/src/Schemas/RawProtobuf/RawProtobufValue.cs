
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
	}
}