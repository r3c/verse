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
				if (!state.FieldType.HasValue)
					state.ReadHeader();

				// Different field index (or end of stream) was met, stop enumeration
				if (fieldIndex != state.FieldIndex)
				{
					element = default;

					return BrowserState.Success;
				}

				// Read field and continue enumeration
				if (!callback(this, state, out element))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}

		public bool ReadToObject<TObject>(RawProtobufReaderState state,
			ILookup<int, ReaderSetter<RawProtobufReaderState, RawProtobufValue, TObject>> fields, ref TObject target)
		{
			// Complex objects are expected to be at top-level (no parent field type) or contained within string type
			if (state.FieldType.GetValueOrDefault(WireType.String) != WireType.String)
			{
				RawProtobufReaderSession.Skip(state);

				return true;
			}

			var subItem = state.FieldType.HasValue ? ProtoReader.StartSubItem(state.Reader) : (SubItemToken?) null;

			state.ClearHeader();

			while (true)
			{
				if (!state.FieldType.HasValue)
					state.ReadHeader();

				// Stop reading complex object when no more field can be read
				if (state.FieldIndex <= 0)
				{
					state.ClearHeader();

					if (subItem.HasValue)
						ProtoReader.EndSubItem(subItem.Value, state.Reader);

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

				if (!(field.Value?.Invoke(this, state, ref target) ?? RawProtobufReaderSession.Skip(state)))
					return false;
			}
		}

		public bool ReadToValue(RawProtobufReaderState state, out RawProtobufValue value)
		{
			var fieldType = state.FieldType.GetValueOrDefault();

			state.ClearHeader();

			switch (fieldType)
			{
				case WireType.Fixed32:
					value = new RawProtobufValue(state.Reader.ReadInt32(), RawProtobufStorage.Fixed32);

					return true;

				case WireType.Fixed64:
					value = new RawProtobufValue(state.Reader.ReadInt64(), RawProtobufStorage.Fixed64);

					return true;

				case WireType.String:
					value = new RawProtobufValue(state.Reader.ReadString(), RawProtobufStorage.String);

					return true;

				case WireType.Variant:
					value = new RawProtobufValue(state.Reader.ReadInt64(), RawProtobufStorage.Variant);

					return true;

				default:
					state.Error($"unsupported wire type {state.FieldType} for field {state.FieldIndex}");

					value = default;

					return false;
			}
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
			state.Reader.SkipField();
			state.ClearHeader();

			return true;
		}
	}
}
