using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufWriterState
	{
		public int FieldIndex;

		private readonly MemoryStream buffer;

		private readonly ErrorEvent error;

		private readonly bool noZigZagEncoding;

		private readonly Stream stream;

		public RawProtobufWriterState(Stream stream, ErrorEvent error, bool noZigZagEncoding)
		{
			this.FieldIndex = 0;

			this.buffer = new MemoryStream();
			this.error = error;
			this.noZigZagEncoding = noZigZagEncoding;
			this.stream = stream;
		}

		public void Flush()
		{
			this.buffer.Position = 0;
			this.buffer.CopyTo(this.stream);
		}

		public void Key(string key)
		{
			if (!int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fieldIndex))
			{
				this.Error($"invalid field name {key}");

				// FIXME: should return false and stop serialization
				return;
			}

			this.FieldIndex = fieldIndex;
		}

		public long? ObjectBegin()
		{
			// If no field index is available it means we're writing top-level object that doesn't require a header
			if (this.FieldIndex <= 0)
				return null;

			this.WriteHeader(this.FieldIndex, RawProtobufWireType.String);

			var marker = this.buffer.Position;

			this.WriteVarInt(0); // Write 1-byte placeholder for object length

			return marker;
		}

		public unsafe void ObjectEnd(long? marker)
		{
			// If marker has no value it means we were writing top-level object so there is no length to be updated
			if (!marker.HasValue)
				return;

			var position = this.buffer.Position;

			// Length of object in bytes (current position minus marker and 1-byte placeholder)
			var length = position - marker.Value - 1;

			// Number of bytes required to encode "length" as a varint
			var bytes = (int) Math.Max(Math.Ceiling(Math.Log(length + 1, 128)), 1);

			// If length requires more than 1 byte to be written we need to shift data to make room for it
			if (bytes > 1)
			{
				this.buffer.Capacity = Math.Max((int) position + bytes - 1, this.buffer.Capacity);
				this.buffer.SetLength(position + bytes - 1);

				fixed (byte* cursor = &this.buffer.GetBuffer()[marker.Value + 1])
				{
					var source = cursor + length;
					var target = cursor + length + bytes - 1;

					for (var i = length; i-- > 0;)
						*--target = *--source;
				}
			}

			// Write length and restore original position in stream
			this.buffer.Position = marker.Value;

			this.WriteVarInt(length);

			this.buffer.Position = position + bytes - 1;
		}

		public void Value(RawProtobufValue value)
		{
			this.WriteHeader(this.FieldIndex, value.Storage);

			switch (value.Storage)
			{
				case RawProtobufWireType.Fixed32:
					this.buffer.WriteByte((byte)((value.Number >> 0) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 8) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 16) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 24) & 255));

					break;

				case RawProtobufWireType.Fixed64:
					this.buffer.WriteByte((byte)((value.Number >> 0) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 8) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 16) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 24) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 32) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 40) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 48) & 255));
					this.buffer.WriteByte((byte)((value.Number >> 56) & 255));

					break;

				case RawProtobufWireType.String:
					var buffer = Encoding.UTF8.GetBytes(value.String);

					this.WriteVarInt(buffer.Length);

					this.buffer.Write(buffer, 0, buffer.Length);

					break;

				case RawProtobufWireType.VarInt:
					// See: https://developers.google.com/protocol-buffers/docs/encoding
					var number = this.noZigZagEncoding ? value.Number : (value.Number << 1) ^ (value.Number >> 31);

					this.WriteVarInt(number);

					break;
			}
		}

		private void Error(string message)
		{
			this.error((int) this.buffer.Position, message);
		}

		private void WriteHeader(int fieldIndex, RawProtobufWireType fieldType)
		{
			// Write field type and first 4 bits of field index
			var wireType = (int) fieldType & 7;

			this.buffer.WriteByte((byte) (wireType | ((fieldIndex & 15) << 3) | (fieldIndex >= 16 ? 128 : 0)));

			fieldIndex >>= 4;

			// Write remaining part of field index if any
			while (fieldIndex > 0)
			{
				this.buffer.WriteByte((byte) ((fieldIndex & 127) | (fieldIndex >= 128 ? 128 : 0)));

				fieldIndex >>= 7;
			}
		}

		private unsafe void WriteVarInt(long value)
		{
			var number = *(ulong*) &value;

			do
			{
				this.buffer.WriteByte((byte) ((number & 127) | (number >= 128 ? 128u : 0u)));

				number >>= 7;
			} while (number > 0);
		}
	}
}
