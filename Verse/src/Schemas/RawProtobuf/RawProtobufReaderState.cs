using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufReaderState
	{
		public readonly ProtoReader Reader;

		public int FieldIndex;

		public WireType? FieldType;

		private readonly DecodeError error;

		public RawProtobufReaderState(Stream stream, DecodeError error)
		{
			this.error = error;

			this.FieldIndex = 0;
			this.FieldType = null;
			this.Reader = new ProtoReader(stream, TypeModel.Create(), null);
		}

		public void ClearHeader()
		{
			this.FieldIndex = 0;
			this.FieldType = null;
		}

		public void ReadHeader()
		{
			this.FieldIndex = this.Reader.ReadFieldHeader();
			this.FieldType = this.Reader.WireType;
		}

		public void Error(string message)
		{
			this.error(this.Reader.Position, message);
		}
	}
}
