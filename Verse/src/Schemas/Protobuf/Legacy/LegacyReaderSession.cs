using System.Globalization;
using System.IO;
using ProtoBuf;
using Verse.DecoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf.Legacy
{
	internal class LegacyReaderSession : IReaderSession<LegacyReaderState, ProtobufValue>
	{
		public BrowserMove<TElement> ReadToArray<TElement>(LegacyReaderState state, ReaderCallback<LegacyReaderState, ProtobufValue, TElement> callback)
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

		public bool ReadToObject<TObject>(LegacyReaderState state,
			ILookup<int, ReaderSetter<LegacyReaderState, ProtobufValue, TObject>> fields, ref TObject target)
		{
			// Complex objects are expected to be at top-level (no parent field type) or contained within string type
			if (state.FieldType.GetValueOrDefault(ProtoBuf.WireType.String) != ProtoBuf.WireType.String)
			{
				LegacyReaderSession.Skip(state);

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

				if (!(field.Value?.Invoke(this, state, ref target) ?? LegacyReaderSession.Skip(state)))
					return false;
			}
		}

		public bool ReadToValue(LegacyReaderState state, out ProtobufValue value)
		{
			var fieldType = state.FieldType.GetValueOrDefault();

			state.ClearHeader();

			switch (fieldType)
			{
				case ProtoBuf.WireType.Fixed32:
					value = new ProtobufValue(state.Reader.ReadInt32());

					return true;

				case ProtoBuf.WireType.Fixed64:
				case ProtoBuf.WireType.Variant:
					value = new ProtobufValue(state.Reader.ReadInt64());

					return true;

				case ProtoBuf.WireType.String:
					value = new ProtobufValue(state.Reader.ReadString());

					return true;

				default:
					state.Error($"unsupported wire type {state.FieldType} for field {state.FieldIndex}");

					value = default;

					return false;
			}
		}

		public bool Start(Stream stream, DecodeError error, out LegacyReaderState state)
		{
			state = new LegacyReaderState(stream, error);

			return true;
		}

		public void Stop(LegacyReaderState state)
		{
		}

		private static bool Skip(LegacyReaderState state)
		{
			state.Reader.SkipField();
			state.ClearHeader();

			return true;
		}
	}
}
