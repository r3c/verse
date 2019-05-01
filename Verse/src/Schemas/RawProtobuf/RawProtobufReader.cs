using System;
using System.Globalization;
using System.IO;
using Verse.DecoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufReader : IReader<RawProtobufReaderState, RawProtobufValue>
	{
		private readonly bool noZigZagEncoding;

		public RawProtobufReader(bool noZigZagEncoding)
		{
			this.noZigZagEncoding = noZigZagEncoding;
		}

		public BrowserMove<TElement> ReadToArray<TElement>(RawProtobufReaderState state, Func<TElement> constructor,
			ReaderCallback<RawProtobufReaderState, RawProtobufValue, TElement> callback)
		{
			var firstIndex = state.FieldIndex;

			return (int index, out TElement element) =>
			{
				// Read next field header if required so we know whether it's still part of the same array or not
				if (index > 0)
				{
					state.ReadHeader();

					// Different field index (or end of stream) was met, stop enumeration
					if (firstIndex != state.FieldIndex)
					{
						element = default;

						return BrowserState.Success;
					}
				}

				// Read field and continue enumeration if we're still reading elements sharing the same field index
				element = constructor();

				return callback(this, state, ref element) ? BrowserState.Continue : BrowserState.Failure;
			};
		}

		public bool ReadToObject<TObject>(RawProtobufReaderState state,
			ILookup<int, ReaderCallback<RawProtobufReaderState, RawProtobufValue, TObject>> fields, ref TObject target)
		{
			if (!state.ObjectBegin(out var backup))
				return state.TrySkipValue();

			while (true)
			{
				state.ReadHeader();

				// Stop reading complex object when no more field can be read
				if (state.FieldIndex <= 0)
				{
					state.ObjectEnd(backup);

					return true;
				}

				var field = fields.Follow('_');

				if (state.FieldIndex > 9)
				{
					foreach (var digit in state.FieldIndex.ToString(CultureInfo.InvariantCulture))
						field = field.Follow(digit);
				}
				else
					field = field.Follow((char) ('0' + state.FieldIndex));

				if (!(field.HasValue ? field.Value(this, state, ref target) : state.TrySkipValue()))
					return false;
			}
		}

		public bool ReadToValue(RawProtobufReaderState state, out RawProtobufValue value)
		{
			return state.TryReadValue(out value);
		}

		public RawProtobufReaderState Start(Stream stream, ErrorEvent error)
		{
			return new RawProtobufReaderState(stream, error, this.noZigZagEncoding);
		}

		public void Stop(RawProtobufReaderState state)
		{
		}
	}
}
