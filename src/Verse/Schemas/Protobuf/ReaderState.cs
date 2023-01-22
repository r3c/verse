using System.Globalization;
using System.IO;

namespace Verse.Schemas.Protobuf;

internal class ReaderState
{
    public readonly Stream Stream;

    public ProtobufValue Value;

    private readonly ErrorEvent error;

    public ReaderState(Stream stream, ErrorEvent error)
    {
        this.error = error;
        Stream = stream;
        Value = ProtobufValue.Empty;
    }

    public void RaiseError(string format, params object[] args)
    {
        error((int)Stream.Position, string.Format(CultureInfo.InvariantCulture, format, args));
    }
}