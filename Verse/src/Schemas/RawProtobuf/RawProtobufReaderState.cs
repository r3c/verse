using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufReaderState
	{
		public int FieldIndex;

		private readonly DecodeError error;

		private WireType? fieldType;

		private readonly ProtoReader reader;

		public RawProtobufReaderState(Stream stream, DecodeError error)
		{
			this.error = error;

			this.FieldIndex = 0;
			this.fieldType = null;
			this.reader = new ProtoReader(stream, TypeModel.Create(), null);
		}

		public void Error(string message)
		{
			this.error(this.reader.Position, message);
		}

		public bool ObjectBegin(out SubItemToken? subItem)
		{
			// Complex objects are expected to be at top-level (no parent field type) or contained within string type
			if (this.fieldType.GetValueOrDefault(WireType.String) != WireType.String)
			{
				subItem = default;

				return false;
			}

			subItem = this.fieldType.HasValue ? ProtoReader.StartSubItem(this.reader) : (SubItemToken?) null;

			this.ClearHeader();

			return true;
		}

		public void ObjectEnd(SubItemToken? subItem)
		{
			this.ClearHeader();

			if (subItem.HasValue)
				ProtoReader.EndSubItem(subItem.Value, this.reader);
		}

		public void ReadHeader()
		{
			if (this.fieldType.HasValue)
				return;

			this.FieldIndex = this.reader.ReadFieldHeader();
			this.fieldType = this.reader.WireType;
		}

		public bool TryReadValue(out RawProtobufValue value)
		{
			var valueFieldType = this.fieldType.GetValueOrDefault();

			this.ClearHeader();

			switch (valueFieldType)
			{
				case WireType.Fixed32:
					value = new RawProtobufValue(this.reader.ReadInt32(), RawProtobufStorage.Fixed32);

					return true;

				case WireType.Fixed64:
					value = new RawProtobufValue(this.reader.ReadInt64(), RawProtobufStorage.Fixed64);

					return true;

				case WireType.String:
					value = new RawProtobufValue(this.reader.ReadString(), RawProtobufStorage.String);

					return true;

				case WireType.Variant:
					value = new RawProtobufValue(this.reader.ReadInt64(), RawProtobufStorage.Variant);

					return true;

				default:
					this.Error($"unsupported wire type {this.fieldType} for field {this.FieldIndex}");

					value = default;

					return false;
			}
		}

		public void SkipField()
		{
			this.reader.SkipField();
			this.ClearHeader();
		}

		private void ClearHeader()
		{
			this.FieldIndex = 0;
			this.fieldType = null;
		}
	}
}
