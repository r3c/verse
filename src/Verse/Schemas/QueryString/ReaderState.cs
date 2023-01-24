using System.IO;
using System.Text;

namespace Verse.Schemas.QueryString;

internal class ReaderState
{
    public int Current;

    public readonly Encoding Encoding;

    public QueryStringLocation Location;

    private readonly ErrorEvent _error;

    private int _position;

    private readonly StreamReader _reader;

    public ReaderState(Stream stream, Encoding encoding, ErrorEvent error)
    {
        Current = 0;
        Encoding = encoding;

        _error = error;
        _position = 0;
        _reader = new StreamReader(stream, encoding);

        Pull();

        Location = QueryStringLocation.Sequence;
    }

    public void Error(string message)
    {
        _error(_position, message);
    }

    public void Pull()
    {
        Current = _reader.Read();

        ++_position;
    }
}