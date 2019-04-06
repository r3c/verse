using System;
using System.Collections.Generic;
using ProtoBuf;
using Verse.DecoderDescriptors.Base;
using Verse.DecoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf.Legacy
{
	class LegacyReader<TEntity> : TreeReader<LegacyReaderState, TEntity, ProtobufValue>
	{
		private static readonly LegacyReader<TEntity> emptyReader = new LegacyReader<TEntity>();

		private readonly List<EntityReader<LegacyReaderState, TEntity>> arrayFields = new List<EntityReader<LegacyReaderState, TEntity>>();

		private readonly List<EntityReader<LegacyReaderState, TEntity>> objectFields = new List<EntityReader<LegacyReaderState, TEntity>>();

		public override TreeReader<LegacyReaderState, TOther, ProtobufValue> Create<TOther>()
		{
			return new LegacyReader<TOther>();
		}

		public override TreeReader<LegacyReaderState, TField, ProtobufValue> HasField<TField>(string name, EntityReader<LegacyReaderState, TEntity> enter)
		{
			List<EntityReader<LegacyReaderState, TEntity>> fields;

			if (int.TryParse(name, out var index))
				fields = this.arrayFields;
			else if (name.Length > 0 && name[0] == '_' && int.TryParse(name.Substring(1), out index))
				fields = this.objectFields;
			else
				throw new ArgumentOutOfRangeException("name", name, "Protobuf schema only supports _X and X as field names, where X is an integer");

			while (fields.Count <= index)
				fields.Add(null);

			fields[index] = enter;

			return new LegacyReader<TField>();
		}

		public override bool Read(ref TEntity entity, LegacyReaderState state)
		{
			switch (state.ReadingAction)
			{
				case LegacyReaderState.ReadingActionType.UseHeader:
					return this.FollowNode(state.Reader.FieldNumber, ref entity, state);

				case LegacyReaderState.ReadingActionType.ReadHeader:
					while (state.ReadHeader(out var fieldIndex))
					{
						state.AddObject(fieldIndex);

						if (!this.FollowNode(fieldIndex, ref entity, state))
							return false;
					}

					return true;

				default:
					state.ReadingAction = LegacyReaderState.ReadingActionType.ReadHeader;

					if (this.IsArray)
						return this.ReadArray(ref entity, state);

					if (!this.IsValue)
					{
						// if it's not object, ignore
						if ((state.Reader.WireType != ProtoBuf.WireType.StartGroup && state.Reader.WireType != ProtoBuf.WireType.String) ||
							(this.arrayFields.Count == 0 && this.objectFields.Count == 0))
						{
							state.Reader.SkipField();

							return true;
						}

						return this.ReadObjectValue(ref entity, state);
					}

					switch (state.Reader.WireType)
					{
						case ProtoBuf.WireType.Fixed32:
							return this.ReadValue(ref entity, new ProtobufValue(state.Reader.ReadSingle()));

						case ProtoBuf.WireType.Fixed64:
							return this.ReadValue(ref entity, new ProtobufValue(state.Reader.ReadDouble()));

						case ProtoBuf.WireType.String:
							return this.ReadValue(ref entity, new ProtobufValue(state.Reader.ReadString()));

						case ProtoBuf.WireType.Variant:
							return this.ReadValue(ref entity, new ProtobufValue(state.Reader.ReadInt64()));
					}

					state.Error("wire type not supported, skipped");
					state.Reader.SkipField();

					return true;
			}
		}

		public override BrowserMove<TEntity> ReadItems(Func<TEntity> constructor, LegacyReaderState state)
		{
			switch (state.Reader.WireType)
			{
				case ProtoBuf.WireType.StartGroup:
				case ProtoBuf.WireType.String:
					return this.ReadSubItemArray(constructor, state);

				default:
					return this.ReadValueArray(constructor, state);
			}
		}

		private static bool Ignore(LegacyReaderState state)
		{
			var dummy = default(TEntity);

			return LegacyReader<TEntity>.emptyReader.Read(ref dummy, state);
		}

		private bool FollowNode(int fieldIndex, ref TEntity target, LegacyReaderState state)
		{
			state.ReadingAction = LegacyReaderState.ReadingActionType.ReadValue;

			return fieldIndex < this.objectFields.Count && this.objectFields[fieldIndex] != null
				? this.objectFields[fieldIndex](state, ref target)
				: LegacyReader<TEntity>.Ignore(state);
		}

		private bool ReadObjectValue(ref TEntity target, LegacyReaderState state)
		{
			var lastSubItem = ProtoReader.StartSubItem(state.Reader);

			if (!state.EnterObject(out var visitCount))
				return false;

			while (ProtoReader.HasSubValue(ProtoBuf.WireType.None, state.Reader))
			{
				state.ReadHeader(out var fieldIndex);
				state.AddObject(fieldIndex);

				if (visitCount == 1 && fieldIndex < this.objectFields.Count && this.objectFields[fieldIndex] != null)
				{
					state.ReadingAction = LegacyReaderState.ReadingActionType.ReadValue;

					if (!this.objectFields[fieldIndex](state, ref target))
						return false;
				}
				else
				{
					int index = visitCount - 1;

					state.ReadingAction = LegacyReaderState.ReadingActionType.UseHeader;

					if (!(index < this.arrayFields.Count && this.arrayFields[index] != null
						? this.arrayFields[index](state, ref target)
						: LegacyReader<TEntity>.Ignore(state)))
						return false;
				}
			}

			state.LeaveObject();

			ProtoReader.EndSubItem(lastSubItem, state.Reader);

			return true;
		}

		private BrowserMove<TEntity> ReadSubItemArray(Func<TEntity> constructor, LegacyReaderState state)
		{
			int dummy;

			if (this.IsValue)
				return this.ReadValueArray(constructor, state);

			state.ReadingAction = LegacyReaderState.ReadingActionType.ReadHeader;

			var lastSubItem = ProtoReader.StartSubItem(state.Reader);

			if (!state.EnterObject(out dummy))
			{
				return (int index, out TEntity current) =>
				{
					current = default;

					return BrowserState.Failure;
				};
			}

			return (int index, out TEntity current) =>
			{
				state.AddObject(index);

				if (!ProtoReader.HasSubValue(ProtoBuf.WireType.None, state.Reader))
				{
					current = default;

					state.LeaveObject();

					ProtoReader.EndSubItem(lastSubItem, state.Reader);

					return BrowserState.Success;
				}

				current = constructor();

				if (this.Read(ref current, state))
					return BrowserState.Continue;

				current = default;

				return BrowserState.Failure;
			};
		}

		private BrowserMove<TEntity> ReadValueArray(Func<TEntity> constructor, LegacyReaderState state)
		{
			state.ReadingAction = LegacyReaderState.ReadingActionType.ReadValue;

			return (int index, out TEntity current) =>
			{
				state.AddObject(index);

				if (index > 0)
				{
					current = default;

					return BrowserState.Success;
				}

				current = constructor();

				if (!this.Read(ref current, state))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}
	}
}
