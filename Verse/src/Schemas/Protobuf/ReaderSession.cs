using System;
using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf.Legacy
{
	static class ReaderSessionHelper
	{
		public static uint ReadInt32(ReaderState state)
		{
			uint u32;

			u32 = (uint)state.Stream.ReadByte();
			u32 += (uint)state.Stream.ReadByte() << 8;
			u32 += (uint)state.Stream.ReadByte() << 16;
			u32 += (uint)state.Stream.ReadByte() << 24;

			return u32;
		}

		public static ulong ReadInt64(ReaderState state)
		{
			ulong u64;

			u64 = (ulong)state.Stream.ReadByte();
			u64 += (ulong)state.Stream.ReadByte() << 8;
			u64 += (ulong)state.Stream.ReadByte() << 16;
			u64 += (ulong)state.Stream.ReadByte() << 24;
			u64 += (ulong)state.Stream.ReadByte() << 32;
			u64 += (ulong)state.Stream.ReadByte() << 40;
			u64 += (ulong)state.Stream.ReadByte() << 48;
			u64 += (ulong)state.Stream.ReadByte() << 56;

			return u64;
		}

		public static ulong ReadVarInt(ReaderState state)
		{
			byte current;
			var shift = 0;
			var u64 = 0UL;

			do
			{
				current = (byte)state.Stream.ReadByte();
				u64 += (current & 127u) << shift;
				shift += 7;
			}
			while ((current & 128u) != 0);

			return u64;
		}
	}

	class ReaderSession : IReaderSession<ReaderState, ProtobufValue>
	{
		public BrowserMove<TElement> ReadToArray<TElement>(ReaderState state, ReaderCallback<ReaderState, ProtobufValue, TElement> callback)
		{
			throw new NotImplementedException();
		}

		public bool ReadToObject<TObject>(ReaderState state, EntityTree<ReaderSetter<ReaderState, ProtobufValue, TObject>> fields, ref TObject target)
		{
			var current = state.Stream.ReadByte();

			if (current < 0)
				return true;

			// Read field number and wire type
			var index = (current >> 3) & 15;
			var wire = (WireType)(current & 7);

			while ((current & 128) != 0)
			{
				current = state.Stream.ReadByte();
				index = (index << 8) + current;
			}

			// Decode value
            ProtoBinding field;
/*
			if (index >= 0 && index < this.bindings.Length && this.bindings[index].Type != ProtoType.Undefined)
				field = this.bindings[index];
			else if (!this.rejectUnknown)
				field = ProtoBinding.Empty;
			else
			{
				state.RaiseError("field {0} with wire type {1} is unknown", index, wire);

				return false;
			}
*/
            field = default;

			switch (wire)
			{
				case WireType.Fixed32:
					var u32 = ReaderSessionHelper.ReadInt32(state);

					switch (field.Type)
					{
						case ProtoType.Fixed32:
							state.Value = new ProtobufValue(u32);

							break;

						case ProtoType.Float:
							unsafe
							{
								state.Value = new ProtobufValue(*((float*)&u32));
							}

							break;

						case ProtoType.SFixed32:
							state.Value = new ProtobufValue((int)u32);

							break;

						case ProtoType.Undefined:
							break;

						default:
							state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

							return false;
					}

					break;

				case WireType.Fixed64:
					var u64 = ReaderSessionHelper.ReadInt64(state);

					switch (field.Type)
					{
						case ProtoType.Double:
							unsafe
							{
								state.Value = new ProtobufValue(*((double*)&u64));
							}

							break;

						case ProtoType.Fixed64:
							state.Value = new ProtobufValue(u64);

							break;

						case ProtoType.SFixed64:
							state.Value = new ProtobufValue((long)u64);

							break;

						case ProtoType.Undefined:
							break;

						default:
							state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

							return false;
					}

					break;

				case WireType.GroupBegin:
					if (field.Type != ProtoType.Undefined)
					{
						state.RaiseError("groups are not supported");

						return false;
					}

					break;

				case WireType.GroupEnd:
					if (field.Type != ProtoType.Undefined)
					{
						state.RaiseError("groups are not supported");

						return false;
					}

					break;

				case WireType.LengthDelimited:
					switch (field.Type)
					{
						case ProtoType.Custom:
							throw new NotImplementedException();

						case ProtoType.String:
							var length = ReaderSessionHelper.ReadVarInt(state);
/*
							if (length > this.maximumLength)
							{
								state.RaiseError("number of bytes in field {0} ({1}) exceeds allowed maximum ({2})", field.Name, length, this.maximumLength);

								return false;
							}
*/
							var buffer = new byte[length];

							if (state.Stream.Read(buffer, 0, (int)length) != (int)length)
								return false;

							state.Value = new ProtobufValue(Encoding.UTF8.GetString(buffer));

							break;

						case ProtoType.Undefined:
							break;

						default:
							state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

							return false;
					}

					return false;

				case WireType.VarInt:
					var varint = ReaderSessionHelper.ReadVarInt(state);

					switch (field.Type)
					{
						case ProtoType.Boolean:
							state.Value = new ProtobufValue(varint != 0);

							break;

						case ProtoType.Int32:
							state.Value = new ProtobufValue((int)varint);

							break;

						case ProtoType.Int64:
							state.Value = new ProtobufValue((long)varint);

							break;

						case ProtoType.SInt32:
						case ProtoType.SInt64:
							state.Value = new ProtobufValue((-(long)(varint & 1)) ^ (long)(varint >> 1));

							break;

						case ProtoType.UInt32:
						case ProtoType.UInt64:
							state.Value = new ProtobufValue(varint);

							break;

						case ProtoType.Undefined:
							break;

						default:
							state.RaiseError("field {0} is incompatible with wire type {1}", field.Name, wire);

							return false;
					}

					break;

				default:
					state.RaiseError("field {0} has unsupported wire type {1}", field.Name, wire);

					return false;
			}
/*
			return this.fields[index] != null
				? this.fields[index](state, ref entity)
				: Reader<TObject>.Ignore(state);
*/
            return false;
		}

		public bool ReadToValue(ReaderState state, out ProtobufValue value)
		{
			throw new NotImplementedException();
		}

		public bool Start(Stream stream, DecodeError error, out ReaderState state)
        {
            state = new ReaderState(stream, error);

            return true;
        }

        public void Stop(ReaderState state)
        {
        }
	}
}
