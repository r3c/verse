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

			FieldIndex = 0;

			boundary = null;
			fieldType = null;
			position = 0;
		}

		public bool ObjectBegin(out int? backup)
		{
			// Complex objects are expected to be at top-level (no parent field type) or contained within string type
			if (fieldType.GetValueOrDefault(RawProtobufWireType.String) != RawProtobufWireType.String)
			{
				backup = default;

				return false;
			}

			if (fieldType.HasValue)
			{
				backup = boundary;

				boundary = position + (int) ReadVarInt();
			}
			else
				backup = null;

			return true;
		}

		public void ObjectEnd(int? backup)
		{
			if (position < boundary.GetValueOrDefault())
				Error("sub-message was not read entirely");

			boundary = backup;
		}

		public void ReadHeader()
		{
			if (boundary.HasValue && position >= boundary.Value)
			{
				FieldIndex = 0;
				this.fieldType = null;

				return;
			}

			var current = stream.ReadByte();

			++position;

			if (current < 0)
			{
				FieldIndex = 0;
				this.fieldType = null;

				return;
			}

			var fieldIndex = (current >> 3) & 15;
			var fieldType = (RawProtobufWireType)(current & 7);
			var shift = 4;

			while ((current & 128) != 0)
			{
				current = stream.ReadByte();

				position += 1;

				fieldIndex += (current & 127) << shift;
				shift += 7;
			}

			FieldIndex = fieldIndex;
			this.fieldType = fieldType;
		}

		public bool TryReadValue(out RawProtobufValue value)
		{
			switch (fieldType.GetValueOrDefault())
			{
				case RawProtobufWireType.Fixed32:
					var fixed32 = 0;

					fixed32 += stream.ReadByte();
					fixed32 += stream.ReadByte() << 8;
					fixed32 += stream.ReadByte() << 16;
					fixed32 += stream.ReadByte() << 24;

					value = new RawProtobufValue(fixed32, RawProtobufWireType.Fixed32);

					position += 4;

					return true;

				case RawProtobufWireType.Fixed64:
					var fixed64 = 0L;

					fixed64 += stream.ReadByte();
					fixed64 += (long) stream.ReadByte() << 8;
					fixed64 += (long) stream.ReadByte() << 16;
					fixed64 += (long) stream.ReadByte() << 24;
					fixed64 += (long) stream.ReadByte() << 32;
					fixed64 += (long) stream.ReadByte() << 40;
					fixed64 += (long) stream.ReadByte() << 48;
					fixed64 += (long) stream.ReadByte() << 56;

					position += 8;

					value = new RawProtobufValue(fixed64, RawProtobufWireType.Fixed64);

					return true;

				case RawProtobufWireType.String:
					var length = (int) ReadVarInt();

					if (length > StringMaxLength)
					{
						Error($"string field exceeds maximum length of {StringMaxLength}");

						value = default;

						return false;
					}

					var buffer = new byte[length];

					if (stream.Read(buffer, 0, length) != length)
					{
						Error($"could not read string of length {length}");

						value = default;

						return false;
					}

					position += length;

					value = new RawProtobufValue(Encoding.UTF8.GetString(buffer), RawProtobufWireType.String);

					return true;

				case RawProtobufWireType.VarInt:
					var varint = ReadVarInt();

					// See: https://developers.google.com/protocol-buffers/docs/encoding
					var number = noZigZagEncoding ? varint : -(varint & 1) ^ (varint >> 1);

					value = new RawProtobufValue(number, RawProtobufWireType.VarInt);

					return true;

				default:
					Error($"unsupported wire type {fieldType}");

					value = default;

					return false;
			}
		}

		public bool TrySkipValue()
		{
			switch (fieldType.GetValueOrDefault())
			{
				case RawProtobufWireType.Fixed32:
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					position += 4;

					return true;

				case RawProtobufWireType.Fixed64:
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					position += 8;

					return true;

				case RawProtobufWireType.String:
					var length = (int) ReadVarInt();

					if (length > StringMaxLength)
					{
						Error($"string field exceeds maximum length of {StringMaxLength}");

						return false;
					}

					while (length-- > 0)
					{
						stream.ReadByte();
						position += 1;
					}

					return true;

				case RawProtobufWireType.VarInt:
					ReadVarInt();

					return true;

				default:
					Error($"unsupported wire type {fieldType}");

					return false;
			}
		}

		private void Error(string message)
		{
			error(position, message);
		}

		private unsafe long ReadVarInt()
		{
			byte current;
			var shift = 0;
			var value = 0UL;

			do
			{
				current = (byte) stream.ReadByte();

				position += 1;

				value += (ulong) (current & 127u) << shift;
				shift += 7;
			} while ((current & 128) != 0);

			return *(long*) &value;
		}
	}
}
