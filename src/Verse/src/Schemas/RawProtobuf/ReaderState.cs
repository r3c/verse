using System.IO;
using System.Text;

namespace Verse.Schemas.RawProtobuf
{
	internal class ReaderState
	{
		private const int StringMaxLength = 65536;

		/// <summary>
		/// Index of field read from last header.
		/// </summary>
		public int FieldIndex;

		/// <summary>
		/// Absolute offset to end of the object being read, if any.
		/// </summary>
		private int? boundary;

		/// <summary>
		/// Type of field read from last header.
		/// </summary>
		private RawProtobufWireType? fieldType;

		/// <summary>
		/// Current byte offset in stream being read.
		/// </summary>
		private int position;

		private readonly ErrorEvent error;

		private readonly bool noZigZagEncoding;

		private readonly Stream stream;

		public ReaderState(Stream stream, ErrorEvent error, bool noZigZagEncoding)
		{
			this.error = error;
			this.noZigZagEncoding = noZigZagEncoding;
			this.stream = stream;

			this.FieldIndex = 0;

			this.boundary = null;
			this.fieldType = null;
			this.position = 0;
		}

		public bool ObjectBegin(out int? backup)
		{
			// Complex objects are expected to be at top-level (no parent field type) or contained within string type
			if (this.fieldType.GetValueOrDefault(RawProtobufWireType.String) != RawProtobufWireType.String)
			{
				backup = default;

				return false;
			}

			if (this.fieldType.HasValue)
			{
				backup = this.boundary;

				this.boundary = this.position + (int) this.ReadVarInt();
			}
			else
				backup = null;

			return true;
		}

		public void ObjectEnd(int? backup)
		{
			if (this.position < this.boundary.GetValueOrDefault())
				this.Error("sub-message was not read entirely");

			this.boundary = backup;
		}

		public void ReadHeader()
		{
			if (this.boundary.HasValue && this.position >= this.boundary.Value)
			{
				this.FieldIndex = 0;
				this.fieldType = null;

				return;
			}

			var current = this.stream.ReadByte();

			++this.position;

			if (current < 0)
			{
				this.FieldIndex = 0;
				this.fieldType = null;

				return;
			}

			var fieldIndex = (current >> 3) & 15;
			var fieldType = (RawProtobufWireType)(current & 7);
			var shift = 4;

			while ((current & 128) != 0)
			{
				current = this.stream.ReadByte();

				this.position += 1;

				fieldIndex += (current & 127) << shift;
				shift += 7;
			}

			this.FieldIndex = fieldIndex;
			this.fieldType = fieldType;
		}

		public bool TryReadValue(out RawProtobufValue value)
		{
			switch (this.fieldType.GetValueOrDefault())
			{
				case RawProtobufWireType.Fixed32:
					var fixed32 = 0;

					fixed32 += this.stream.ReadByte();
					fixed32 += this.stream.ReadByte() << 8;
					fixed32 += this.stream.ReadByte() << 16;
					fixed32 += this.stream.ReadByte() << 24;

					value = new RawProtobufValue(fixed32, RawProtobufWireType.Fixed32);

					this.position += 4;

					return true;

				case RawProtobufWireType.Fixed64:
					var fixed64 = 0L;

					fixed64 += this.stream.ReadByte();
					fixed64 += (long) this.stream.ReadByte() << 8;
					fixed64 += (long) this.stream.ReadByte() << 16;
					fixed64 += (long) this.stream.ReadByte() << 24;
					fixed64 += (long) this.stream.ReadByte() << 32;
					fixed64 += (long) this.stream.ReadByte() << 40;
					fixed64 += (long) this.stream.ReadByte() << 48;
					fixed64 += (long) this.stream.ReadByte() << 56;

					this.position += 8;

					value = new RawProtobufValue(fixed64, RawProtobufWireType.Fixed64);

					return true;

				case RawProtobufWireType.String:
					var length = (int) this.ReadVarInt();

					if (length > ReaderState.StringMaxLength)
					{
						this.Error($"string field exceeds maximum length of {ReaderState.StringMaxLength}");

						value = default;

						return false;
					}

					var buffer = new byte[length];

					if (this.stream.Read(buffer, 0, length) != length)
					{
						this.Error($"could not read string of length {length}");

						value = default;

						return false;
					}

					this.position += length;

					value = new RawProtobufValue(Encoding.UTF8.GetString(buffer), RawProtobufWireType.String);

					return true;

				case RawProtobufWireType.VarInt:
					var varint = this.ReadVarInt();

					// See: https://developers.google.com/protocol-buffers/docs/encoding
					var number = this.noZigZagEncoding ? varint : -(varint & 1) ^ (varint >> 1);

					value = new RawProtobufValue(number, RawProtobufWireType.VarInt);

					return true;

				default:
					this.Error($"unsupported wire type {this.fieldType}");

					value = default;

					return false;
			}
		}

		public bool TrySkipValue()
		{
			switch (this.fieldType.GetValueOrDefault())
			{
				case RawProtobufWireType.Fixed32:
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.position += 4;

					return true;

				case RawProtobufWireType.Fixed64:
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.stream.ReadByte();
					this.position += 8;

					return true;

				case RawProtobufWireType.String:
					var length = (int) this.ReadVarInt();

					if (length > ReaderState.StringMaxLength)
					{
						this.Error($"string field exceeds maximum length of {ReaderState.StringMaxLength}");

						return false;
					}

					while (length-- > 0)
					{
						this.stream.ReadByte();
						this.position += 1;
					}

					return true;

				case RawProtobufWireType.VarInt:
					this.ReadVarInt();

					return true;

				default:
					this.Error($"unsupported wire type {this.fieldType}");

					return false;
			}
		}

		private void Error(string message)
		{
			this.error(this.position, message);
		}

		private unsafe long ReadVarInt()
		{
			byte current;
			var shift = 0;
			var value = 0UL;

			do
			{
				current = (byte) this.stream.ReadByte();

				this.position += 1;

				value += (ulong) (current & 127u) << shift;
				shift += 7;
			} while ((current & 128) != 0);

			return *(long*) &value;
		}
	}
}
