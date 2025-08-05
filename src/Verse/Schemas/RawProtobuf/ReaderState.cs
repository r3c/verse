using System.IO;
using System.Text;
using Verse.Formats.RawProtobuf;

namespace Verse.Schemas.RawProtobuf;

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
    private int? _boundary;

    /// <summary>
    /// Type of field read from last header.
    /// </summary>
    private RawProtobufWireType? _fieldType;

    /// <summary>
    /// Current byte offset in stream being read.
    /// </summary>
    private int _position;

    private readonly ErrorEvent _error;

    private readonly bool _noZigZagEncoding;

    private readonly Stream _stream;

    public ReaderState(Stream stream, ErrorEvent error, bool noZigZagEncoding)
    {
        _error = error;
        _noZigZagEncoding = noZigZagEncoding;
        _stream = stream;

        FieldIndex = 0;

        _boundary = null;
        _fieldType = null;
        _position = 0;
    }

    public bool ObjectBegin(out int? backup)
    {
        // Complex objects are expected to be at top-level (no parent field type) or contained within string type
        if (_fieldType.GetValueOrDefault(RawProtobufWireType.String) != RawProtobufWireType.String)
        {
            backup = null;

            return false;
        }

        if (_fieldType.HasValue)
        {
            backup = _boundary;

            _boundary = _position + (int)ReadVarInt();
        }
        else
            backup = null;

        return true;
    }

    public void ObjectEnd(int? backup)
    {
        if (_position < _boundary.GetValueOrDefault())
            Error("sub-message was not read entirely");

        _boundary = backup;
    }

    public void ReadHeader()
    {
        if (_boundary.HasValue && _position >= _boundary.Value)
        {
            FieldIndex = 0;
            _fieldType = null;

            return;
        }

        var current = _stream.ReadByte();

        ++_position;

        if (current < 0)
        {
            FieldIndex = 0;
            _fieldType = null;

            return;
        }

        var fieldIndex = (current >> 3) & 15;
        var fieldType = (RawProtobufWireType)(current & 7);
        var shift = 4;

        while ((current & 128) != 0)
        {
            current = _stream.ReadByte();

            _position += 1;

            fieldIndex += (current & 127) << shift;
            shift += 7;
        }

        FieldIndex = fieldIndex;
        _fieldType = fieldType;
    }

    public bool TryReadValue(out RawProtobufValue value)
    {
        switch (_fieldType.GetValueOrDefault())
        {
            case RawProtobufWireType.Fixed32:
                var fixed32 = 0;

                fixed32 += _stream.ReadByte();
                fixed32 += _stream.ReadByte() << 8;
                fixed32 += _stream.ReadByte() << 16;
                fixed32 += _stream.ReadByte() << 24;

                value = new RawProtobufValue(fixed32, RawProtobufWireType.Fixed32);

                _position += 4;

                return true;

            case RawProtobufWireType.Fixed64:
                var fixed64 = 0L;

                fixed64 += _stream.ReadByte();
                fixed64 += (long)_stream.ReadByte() << 8;
                fixed64 += (long)_stream.ReadByte() << 16;
                fixed64 += (long)_stream.ReadByte() << 24;
                fixed64 += (long)_stream.ReadByte() << 32;
                fixed64 += (long)_stream.ReadByte() << 40;
                fixed64 += (long)_stream.ReadByte() << 48;
                fixed64 += (long)_stream.ReadByte() << 56;

                _position += 8;

                value = new RawProtobufValue(fixed64, RawProtobufWireType.Fixed64);

                return true;

            case RawProtobufWireType.String:
                var length = (int)ReadVarInt();

                if (length > StringMaxLength)
                {
                    Error($"string field exceeds maximum length of {StringMaxLength}");

                    value = default;

                    return false;
                }

                var buffer = new byte[length];

                if (_stream.Read(buffer, 0, length) != length)
                {
                    Error($"could not read string of length {length}");

                    value = default;

                    return false;
                }

                _position += length;

                value = new RawProtobufValue(Encoding.UTF8.GetString(buffer), RawProtobufWireType.String);

                return true;

            case RawProtobufWireType.VarInt:
                var varint = ReadVarInt();

                // See: https://developers.google.com/protocol-buffers/docs/encoding
                var number = _noZigZagEncoding ? varint : -(varint & 1) ^ (varint >> 1);

                value = new RawProtobufValue(number, RawProtobufWireType.VarInt);

                return true;

            default:
                Error($"unsupported wire type {_fieldType}");

                value = default;

                return false;
        }
    }

    public bool TrySkipValue()
    {
        switch (_fieldType.GetValueOrDefault())
        {
            case RawProtobufWireType.Fixed32:
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _position += 4;

                return true;

            case RawProtobufWireType.Fixed64:
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _stream.ReadByte();
                _position += 8;

                return true;

            case RawProtobufWireType.String:
                var length = (int)ReadVarInt();

                if (length > StringMaxLength)
                {
                    Error($"string field exceeds maximum length of {StringMaxLength}");

                    return false;
                }

                while (length-- > 0)
                {
                    _stream.ReadByte();
                    _position += 1;
                }

                return true;

            case RawProtobufWireType.VarInt:
                ReadVarInt();

                return true;

            default:
                Error($"unsupported wire type {_fieldType}");

                return false;
        }
    }

    private void Error(string message)
    {
        _error(_position, message);
    }

    private unsafe long ReadVarInt()
    {
        byte current;
        var shift = 0;
        var value = 0UL;

        do
        {
            current = (byte)_stream.ReadByte();

            _position += 1;

            value += (ulong)(current & 127u) << shift;
            shift += 7;
        }
        while ((current & 128) != 0);

        return *(long*)&value;
    }
}