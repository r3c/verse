using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.Formats.RawProtobuf;

namespace Verse.Schemas.RawProtobuf;

internal class WriterState(Stream stream, ErrorEvent error, bool noZigZagEncoding)
{
    public int FieldIndex;

    private readonly MemoryStream _buffer = new();

    public bool Flush()
    {
        FieldIndex = 0;

        _buffer.Position = 0;
        _buffer.CopyTo(stream);
        _buffer.SetLength(0);

        return true;
    }

    public bool Key(string key)
    {
        if (!int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fieldIndex))
        {
            Error($"invalid field name {key}");

            return false;
        }

        FieldIndex = fieldIndex;

        return true;
    }

    public long? ObjectBegin()
    {
        // If no field index is available it means we're writing top-level object that doesn't require a header
        if (FieldIndex <= 0)
            return null;

        WriteHeader(FieldIndex, RawProtobufWireType.String);

        var marker = _buffer.Position;

        WriteVarInt(0); // Write 1-byte placeholder for object length

        return marker;
    }

    public unsafe void ObjectEnd(long? marker)
    {
        // If marker has no value it means we were writing top-level object so there is no length to be updated
        if (!marker.HasValue)
            return;

        var position = _buffer.Position;

        // Length of object in bytes (current position minus marker and 1-byte placeholder)
        var length = position - marker.Value - 1;

        // Number of bytes required to encode "length" as a varint
        var bytes = (int)Math.Max(Math.Ceiling(Math.Log(length + 1, 128)), 1);

        // If length requires more than 1 byte to be written we need to shift data to make room for it
        if (bytes > 1)
        {
            _buffer.Capacity = Math.Max((int)position + bytes - 1, _buffer.Capacity);
            _buffer.SetLength(position + bytes - 1);

            fixed (byte* cursor = &_buffer.GetBuffer()[marker.Value + 1])
            {
                var source = cursor + length;
                var target = cursor + length + bytes - 1;

                for (var i = length; i-- > 0;)
                    *--target = *--source;
            }
        }

        // Write length and restore original position in stream
        _buffer.Position = marker.Value;

        WriteVarInt(length);

        _buffer.Position = position + bytes - 1;
    }

    public bool Value(RawProtobufValue value)
    {
        WriteHeader(FieldIndex, value.Storage);

        switch (value.Storage)
        {
            case RawProtobufWireType.Fixed32:
                _buffer.WriteByte((byte)(value.Number >> 0 & 255));
                _buffer.WriteByte((byte)(value.Number >> 8 & 255));
                _buffer.WriteByte((byte)(value.Number >> 16 & 255));
                _buffer.WriteByte((byte)(value.Number >> 24 & 255));

                return true;

            case RawProtobufWireType.Fixed64:
                _buffer.WriteByte((byte)(value.Number >> 0 & 255));
                _buffer.WriteByte((byte)(value.Number >> 8 & 255));
                _buffer.WriteByte((byte)(value.Number >> 16 & 255));
                _buffer.WriteByte((byte)(value.Number >> 24 & 255));
                _buffer.WriteByte((byte)(value.Number >> 32 & 255));
                _buffer.WriteByte((byte)(value.Number >> 40 & 255));
                _buffer.WriteByte((byte)(value.Number >> 48 & 255));
                _buffer.WriteByte((byte)(value.Number >> 56 & 255));

                return true;

            case RawProtobufWireType.String:
                var buffer = Encoding.UTF8.GetBytes(value.String);

                WriteVarInt(buffer.Length);

                _buffer.Write(buffer, 0, buffer.Length);

                return true;

            case RawProtobufWireType.VarInt:
                // See: https://developers.google.com/protocol-buffers/docs/encoding
                var number = noZigZagEncoding ? value.Number : value.Number << 1 ^ value.Number >> 31;

                WriteVarInt(number);

                return true;

            default:
                Error($"invalid wire type {value.Storage} for field {value.Number}");

                return false;
        }
    }

    private void Error(string message)
    {
        error((int)_buffer.Position, message);
    }

    private void WriteHeader(int fieldIndex, RawProtobufWireType fieldType)
    {
        // Write field type and first 4 bits of field index
        var wireType = (int)fieldType & 7;

        _buffer.WriteByte((byte)(wireType | (fieldIndex & 15) << 3 | (fieldIndex >= 16 ? 128 : 0)));

        fieldIndex >>= 4;

        // Write remaining part of field index if any
        while (fieldIndex > 0)
        {
            _buffer.WriteByte((byte)(fieldIndex & 127 | (fieldIndex >= 128 ? 128 : 0)));

            fieldIndex >>= 7;
        }
    }

    private unsafe void WriteVarInt(long value)
    {
        var number = *(ulong*)&value;

        do
        {
            _buffer.WriteByte((byte)(number & 127 | (number >= 128 ? 128u : 0u)));

            number >>= 7;
        }
        while (number > 0);
    }
}