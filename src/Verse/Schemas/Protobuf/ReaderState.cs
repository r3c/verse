using System.Globalization;
using System.IO;
using Verse.Formats.Protobuf;

namespace Verse.Schemas.Protobuf;

internal class ReaderState(Stream stream, ErrorEvent error)
{
    public readonly Stream Stream = stream;

    public ProtobufValue Value = ProtobufValue.Empty;

    public void RaiseError(string format, params object[] args)
    {
        error((int)Stream.Position, string.Format(CultureInfo.InvariantCulture, format, args));
    }
}