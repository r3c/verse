using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ProtoBuf;

namespace Verse.Schemas.Protobuf.Legacy
{
	class LegacyWriterState
	{
		private readonly EncodeError error;

		private int fieldIndex;

		private long parentOffset;

		private readonly Stream stream;

		private readonly Stack<SubObjectInstance> subObjectInstances;

		public LegacyWriterState(Stream stream, EncodeError error)
		{
			this.error = error;

			this.fieldIndex = 0;
			this.parentOffset = 0;
			this.stream = stream;
			this.subObjectInstances = new Stack<SubObjectInstance>();
		}

		public void Error(string message)
		{
			long localPosition;

			localPosition = this.subObjectInstances.Count > 0
				? this.subObjectInstances.Peek().Stream.Position
				: 0;

			this.error((int)(this.parentOffset + localPosition), message);
		}

		public void Key(string key)
		{
			this.fieldIndex = int.Parse(key, CultureInfo.InvariantCulture);
		}

		public void ObjectBegin()
		{
			SubObjectInstance subObjectInstance;

			if (this.subObjectInstances.Count > 0)
				this.parentOffset += this.subObjectInstances.Peek().Stream.Position;

			subObjectInstance = new SubObjectInstance(this.fieldIndex);

			this.subObjectInstances.Push(subObjectInstance);
		}

		public bool ObjectEnd()
		{
			SubObjectInstance subObjectInstance;

			if (this.subObjectInstances.Count == 0)
				return false;

			subObjectInstance = this.subObjectInstances.Pop();

			this.parentOffset -= subObjectInstance.Stream.Position;
			this.fieldIndex = subObjectInstance.Index;

			this.CopyToParent(subObjectInstance);

			return true;
		}

		public bool Value(ProtobufValue value)
		{
			ProtoWriter destWriter;

			if (this.subObjectInstances.Count == 0)
				return false;

			destWriter = this.subObjectInstances.Peek().Writer;

			switch (value.Type)
			{
				case ProtobufType.Boolean:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, ProtoBuf.WireType.Variant, destWriter);
					ProtoWriter.WriteBoolean(value.Boolean, destWriter);
					break;

				case ProtobufType.Float32:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, ProtoBuf.WireType.Fixed32, destWriter);
					ProtoWriter.WriteSingle(value.Float32, destWriter);
					break;

				case ProtobufType.Float64:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, ProtoBuf.WireType.Fixed64, destWriter);
					ProtoWriter.WriteDouble(value.Float64, destWriter);
					break;

				case ProtobufType.Signed:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, ProtoBuf.WireType.Variant, destWriter);
					ProtoWriter.WriteInt64(value.Signed, destWriter);
					break;

				case ProtobufType.String:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, ProtoBuf.WireType.String, destWriter);
					ProtoWriter.WriteString(value.String ?? string.Empty, destWriter);
					break;

				case ProtobufType.Unsigned:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, ProtoBuf.WireType.Variant, destWriter);
					ProtoWriter.WriteUInt64(value.Unsigned, destWriter);
					break;

				case ProtobufType.Void:
					// do nothing
					break;
			}

			return true;
		}

		private void CopyToParent(SubObjectInstance subObjectInstance)
		{
			subObjectInstance.Writer.Close();
			subObjectInstance.Stream.Seek(0, SeekOrigin.Begin);

			if (this.subObjectInstances.Count == 0)
			{
				subObjectInstance.Stream.CopyTo(this.stream);
			}
			else
			{
				ProtoWriter destWriter;

				destWriter = this.subObjectInstances.Peek().Writer;

				ProtoWriter.WriteFieldHeader(subObjectInstance.Index, ProtoBuf.WireType.String, destWriter);
				ProtoWriter.WriteBytes(subObjectInstance.Stream.ToArray(), destWriter);
			}
		}
	}
}
