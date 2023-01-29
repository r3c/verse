using System.IO;

namespace Verse.Schemas.Protobuf;

internal class WriterState
{
    public readonly ErrorEvent Error;

    public readonly Stream Stream;

    public WriterState(Stream stream, ErrorEvent error)
    {
        Error = error;
        Stream = stream;
    }
}