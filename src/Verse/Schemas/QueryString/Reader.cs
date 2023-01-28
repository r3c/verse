using System;
using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Tree;
using Verse.LookupNodes;

namespace Verse.Schemas.QueryString;

internal class Reader : IReader<ReaderState, string, char>
{
    private readonly Encoding _encoding;

    public Reader(Encoding encoding)
    {
        _encoding = encoding;
    }

    public ReaderStatus ReadToArray<TElement>(ReaderState state,
        ReaderCallback<ReaderState, string, char, TElement> callback, out ArrayReader<TElement> arrayReader)
    {
        arrayReader = _ => ArrayResult<TElement>.Failure;

        return ReaderStatus.Ignored;
    }

    public ReaderStatus ReadToObject<TObject>(ReaderState state,
        ILookupNode<char, ReaderCallback<ReaderState, string, char, TObject>> root, ref TObject target)
    {
        if (state.Current == -1)
            return ReaderStatus.Succeeded;

        while (true)
        {
            // Parse field name
            var empty = true;

            // FIXME: handle % encoding in field names
            var node = root;

            while (QueryStringCharacter.IsUnreserved(state.Current))
            {
                empty = false;
                node = node.Follow((char) state.Current);

                state.Pull();
            }

            if (empty)
            {
                state.Error("empty field name");

                return ReaderStatus.Failed;
            }

            // Parse field value
            switch (state.Current)
            {
                case '=':
                    state.Pull();
                    state.Location = QueryStringLocation.ValueBegin;

                    if (!(node.HasValue ? node.Value(this, state, ref target) == ReaderStatus.Succeeded : ReadToValue(state, out _) == ReaderStatus.Succeeded))
                        return ReaderStatus.Failed;

                    break;

                default:
                    state.Location = QueryStringLocation.ValueEnd;

                    if (node.HasValue && node.Value(this, state, ref target) != ReaderStatus.Succeeded)
                        return ReaderStatus.Failed;

                    break;
            }

            if (state.Location != QueryStringLocation.ValueEnd)
                throw new InvalidOperationException(
                    "internal error, please report an issue on GitHub: https://github.com/r3c/verse/issues");

            // Expect either field separator or end of stream
            if (state.Current == -1)
                return ReaderStatus.Succeeded;

            if (!QueryStringCharacter.IsSeparator(state.Current))
            {
                state.Error("unexpected character");

                return ReaderStatus.Failed;
            }

            state.Pull();

            // Check for end of stream (in case of dangling separator e.g. "?k&") and resume loop
            if (state.Current == -1)
                return ReaderStatus.Succeeded;

            state.Location = QueryStringLocation.Sequence;
        }
    }

    public ReaderStatus ReadToValue(ReaderState state, out string value)
    {
        switch (state.Location)
        {
            case QueryStringLocation.Sequence:
                var dummy = false;

                value = default!;

                return ReadToObject(state,
                    EmptyLookupNode<char, ReaderCallback<ReaderState, string, char, bool>>.Instance, ref dummy);

            case QueryStringLocation.ValueBegin:
                return ReadValue(state, out value) ? ReaderStatus.Succeeded : ReaderStatus.Failed;

            case QueryStringLocation.ValueEnd:
                value = string.Empty;

                return ReaderStatus.Succeeded;

            default:
                value = default!;

                return ReaderStatus.Failed;
        }
    }

    public ReaderState Start(Stream stream, ErrorEvent error)
    {
        var state = new ReaderState(stream, _encoding, error);

        if (state.Current == '?')
            state.Pull();

        return state;
    }

    public void Stop(ReaderState context)
    {
    }

    private static bool ReadValue(ReaderState state, out string value)
    {
        var buffer = new byte[state.Encoding.GetMaxByteCount(1)];
        var builder = new StringBuilder(32);

        while (true)
        {
            var current = state.Current;

            if (QueryStringCharacter.IsUnreserved(current))
            {
                builder.Append((char)current);

                state.Pull();
            }
            else if (current == '+')
            {
                builder.Append(' ');

                state.Pull();
            }
            else if (current == '%')
            {
                int count;

                for (count = 0; state.Current == '%'; ++count)
                {
                    if (count >= buffer.Length)
                    {
                        value = default!;

                        return false;
                    }

                    state.Pull();

                    if (state.Current == -1)
                    {
                        value = default!;

                        return false;
                    }

                    var hex1 = QueryStringCharacter.HexaToDecimal(state.Current);

                    state.Pull();

                    if (state.Current == -1)
                    {
                        value = default!;

                        return false;
                    }

                    var hex2 = QueryStringCharacter.HexaToDecimal(state.Current);

                    state.Pull();

                    if (hex1 < 0 || hex2 < 0)
                    {
                        value = default!;

                        return false;
                    }

                    buffer[count] = (byte)((hex1 << 4) + hex2);
                }

                builder.Append(state.Encoding.GetChars(buffer, 0, count));
            }
            else
            {
                state.Location = QueryStringLocation.ValueEnd;

                value = builder.ToString();

                return true;
            }
        }	
    }
}