﻿using System.Globalization;
using System.IO;
using Verse.DecoderDescriptors.Tree;
using Verse.Formats.RawProtobuf;

namespace Verse.Schemas.RawProtobuf;

internal class Reader(bool noZigZagEncoding) : IReader<ReaderState, RawProtobufValue, char>
{
    public ReaderStatus ReadToArray<TElement>(ReaderState state,
        ReaderCallback<ReaderState, RawProtobufValue, char, TElement> callback,
        out ArrayReader<TElement> arrayReader)
    {
        var firstIndex = state.FieldIndex;

        arrayReader = index =>
        {
            // Read next field header if required so we know whether it's still part of the same array or not
            if (index > 0)
            {
                state.ReadHeader();

                // Different field index (or end of stream) was met, stop enumeration
                if (firstIndex != state.FieldIndex)
                    return ArrayResult<TElement>.EndOfArray;
            }

            // Read field and continue enumeration if we're still reading elements sharing the same field index
            TElement element = default!;

            return callback(this, state, ref element) != ReaderStatus.Failed
                ? ArrayResult.NextElement(element)
                : ArrayResult<TElement>.Failure;
        };

        return ReaderStatus.Succeeded;
    }

    public ReaderStatus ReadToObject<TObject>(ReaderState state,
        ILookupNode<char, ReaderCallback<ReaderState, RawProtobufValue, char, TObject>> root, ref TObject target)
    {
        if (!state.ObjectBegin(out var backup))
            return state.TrySkipValue() ? ReaderStatus.Succeeded : ReaderStatus.Failed;

        while (true)
        {
            state.ReadHeader();

            // Stop reading complex object when no more field can be read
            if (state.FieldIndex <= 0)
            {
                state.ObjectEnd(backup);

                return ReaderStatus.Succeeded;
            }

            var node = root.Follow('_');

            if (state.FieldIndex > 9)
            {
                foreach (var digit in state.FieldIndex.ToString(CultureInfo.InvariantCulture))
                    node = node.Follow(digit);
            }
            else
                node = node.Follow((char)('0' + state.FieldIndex));

            if (!(node.HasValue ? node.Value(this, state, ref target) == ReaderStatus.Succeeded : state.TrySkipValue()))
                return ReaderStatus.Failed;
        }
    }

    public ReaderStatus ReadToValue(ReaderState state, out RawProtobufValue value)
    {
        return state.TryReadValue(out value) ? ReaderStatus.Succeeded : ReaderStatus.Failed;
    }

    public ReaderState Start(Stream stream, ErrorEvent error)
    {
        return new ReaderState(stream, error, noZigZagEncoding);
    }

    public void Stop(ReaderState state)
    {
    }
}