namespace Verse.Schemas.RawProtobuf
{
	public readonly struct RawProtobufValue
	{
		public readonly long Number;

		public readonly RawProtobufWireType Storage;

		public readonly string String;

		public RawProtobufValue(long number, RawProtobufWireType storage)
		{
			this.Number = number;
			this.Storage = storage;
			this.String = default;
		}

		public RawProtobufValue(string value, RawProtobufWireType storage)
		{
			this.Number = default;
			this.Storage = storage;
			this.String = value;
		}
	}
}
