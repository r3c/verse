using System.Globalization;
using System.IO;
using ProtoBuf;
using Verse.DecoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufReaderSession : IReaderSession<RawProtobufReaderState, RawProtobufValue>
	{
		public BrowserMove<TElement> ReadToArray<TElement>(RawProtobufReaderState state, ReaderCallback<RawProtobufReaderState, RawProtobufValue, TElement> callback)
		{
			var fieldIndex = state.FieldIndex;

			return (int index, out TElement element) =>
			{
				// Read next field header if required so we know whether it's still part of the same array or not
				state.ReadHeader();

				// Read field and continue enumeration if we're still reading elements sharing the same field index
				if (fieldIndex == state.FieldIndex)
					return callback(this, state, out element) ? BrowserState.Continue : BrowserState.Failure;

				// Different field index (or end of stream) was met, stop enumeration
				element = default;

				return BrowserState.Success;
			};
		}

		public bool ReadToObject<TObject>(RawProtobufReaderState state,
			ILookup<int, ReaderSetter<RawProtobufReaderState, RawProtobufValue, TObject>> fields, ref TObject target)
		{
			if (!state.ObjectBegin(out var subItem))
			{
				RawProtobufReaderSession.Skip(state);

				return true;
			}

			while (true)
			{
				state.ReadHeader();

				// Stop reading complex object when no more field can be read
				if (state.FieldIndex <= 0)
				{
					state.ObjectEnd(subItem);

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

				if (!(field.HasValue ? field.Value(this, state, ref target) : RawProtobufReaderSession.Skip(state)))
					return false;
			}
		}

		public bool ReadToValue(RawProtobufReaderState state, out RawProtobufValue value)
		{
			return state.TryReadValue(out value);
		}

		public bool Start(Stream stream, DecodeError error, out RawProtobufReaderState state)
		{
			state = new RawProtobufReaderState(stream, error);

			return true;
		}

		public void Stop(RawProtobufReaderState state)
		{
		}

		private static bool Skip(RawProtobufReaderState state)
		{
			state.SkipField();

			return true;
		}
	}
}
