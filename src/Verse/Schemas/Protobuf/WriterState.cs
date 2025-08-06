using System.IO;

namespace Verse.Schemas.Protobuf;

internal class WriterState(Stream stream, ErrorEvent error)
{
    public readonly ErrorEvent Error = error;

    public readonly Stream Stream = stream;
}