using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ProtoBuf;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufWriterState
	{
		private readonly ErrorEvent error;

		private int fieldIndex;

		private long parentOffset;

		private readonly Stream stream;

		private readonly Stack<SubObjectInstance> subObjectInstances;

		public RawProtobufWriterState(Stream stream, ErrorEvent error)
		{
			this.error = error;

			this.fieldIndex = 0;
			this.parentOffset = 0;
			this.stream = stream;
			this.subObjectInstances = new Stack<SubObjectInstance>();
		}

		public void Error(string message)
		{
		    var localPosition = this.subObjectInstances.Count > 0
		        ? this.subObjectInstances.Peek().Stream.Position
		        : 0;

		    this.error((int)(this.parentOffset + localPosition), message);
		}

		public void Flush()
		{
			foreach (var instance in this.subObjectInstances)
				instance.Writer.Close();
		}

		public void Key(string key)
		{
			this.fieldIndex = int.Parse(key, CultureInfo.InvariantCulture);
		}

		public void ObjectBegin()
		{
		    if (this.subObjectInstances.Count > 0)
				this.parentOffset += this.subObjectInstances.Peek().Stream.Position;

			var subObjectInstance = new SubObjectInstance(this.fieldIndex);

			this.subObjectInstances.Push(subObjectInstance);
		}

		public bool ObjectEnd()
		{
		    if (this.subObjectInstances.Count == 0)
				return false;

			var subObjectInstance = this.subObjectInstances.Pop();

			this.parentOffset -= subObjectInstance.Stream.Position;
			this.fieldIndex = subObjectInstance.Index;

			this.CopyToParent(subObjectInstance);

			return true;
		}

		public bool Value(RawProtobufValue value)
		{
		    if (this.subObjectInstances.Count == 0)
				return false;

			var writer = this.subObjectInstances.Peek().Writer;

			switch (value.Storage)
			{
				case RawProtobufStorage.Fixed32:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, WireType.Fixed32, writer);
					ProtoWriter.WriteInt64(value.Number, writer);
					break;

				case RawProtobufStorage.Fixed64:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, WireType.Fixed64, writer);
					ProtoWriter.WriteInt64(value.Number, writer);
					break;

				case RawProtobufStorage.String:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, WireType.String, writer);
					ProtoWriter.WriteString(value.String, writer);
					break;

				case RawProtobufStorage.Variant:
					ProtoWriter.WriteFieldHeader(this.fieldIndex, WireType.Variant, writer);
					ProtoWriter.WriteInt64(value.Number, writer);
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
			    var destWriter = this.subObjectInstances.Peek().Writer;

				ProtoWriter.WriteFieldHeader(subObjectInstance.Index, WireType.String, destWriter);
				ProtoWriter.WriteBytes(subObjectInstance.Stream.ToArray(), destWriter);
			}
		}
	}
}
