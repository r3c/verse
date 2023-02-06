using System.Globalization;
using System.IO;
using Verse.Formats.Protobuf;

namespace Verse.Schemas.Protobuf;

internal class ReaderState
{
    public readonly Stream Stream;

    public ProtobufValue Value;

    private readonly ErrorEvent _error;

    public ReaderState(Stream stream, ErrorEvent error)
    {
        _error = error;
        Stream = stream;
        Value = ProtobufValue.Empty;
    }

    public void RaiseError(string format, params object[] args)
    {
        _error((int)Stream.Position, string.Format(CultureInfo.InvariantCulture, format, args));
    }
}