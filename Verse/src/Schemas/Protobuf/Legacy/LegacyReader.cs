using System;
using System.Collections.Generic;
using ProtoBuf;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Recurse;

namespace Verse.Schemas.Protobuf.Legacy
{
	class LegacyReader<TEntity> : RecurseReader<TEntity, LegacyReaderState, ProtobufValue>
	{
		#region Attributes

		private EntityReader<TEntity, LegacyReaderState> array = null;

		private static readonly LegacyReader<TEntity> emptyReader = new LegacyReader<TEntity>();

		private readonly List<EntityReader<TEntity, LegacyReaderState>> arrayFields = new List<EntityReader<TEntity, LegacyReaderState>>();

		private readonly List<EntityReader<TEntity, LegacyReaderState>> objectFields = new List<EntityReader<TEntity, LegacyReaderState>>();

		#endregion

		#region Methods / Public

		public override BrowserMove<TEntity> Browse(Func<TEntity> constructor, LegacyReaderState state)
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

		public override RecurseReader<TField, LegacyReaderState, ProtobufValue> HasField<TField>(string name, EntityReader<TEntity, LegacyReaderState> enter)
		{
			List<EntityReader<TEntity, LegacyReaderState>> fields;
			int index;

			if (int.TryParse(name, out index))
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

		public override RecurseReader<TItem, LegacyReaderState, ProtobufValue> HasItems<TItem>(EntityReader<TEntity, LegacyReaderState> enter)
		{
			if (this.array != null)
				throw new InvalidOperationException("can't declare array twice on same descriptor");

			this.array = enter;

			return new LegacyReader<TItem>();
		}

		public override bool Read(ref TEntity entity, LegacyReaderState state)
		{
			int fieldIndex;

			switch (state.ReadingAction)
			{
				case LegacyReaderState.ReadingActionType.UseHeader:
					return this.FollowNode(state.Reader.FieldNumber, ref entity, state);

				case LegacyReaderState.ReadingActionType.ReadHeader:
					while (state.ReadHeader(out fieldIndex))
					{
						state.AddObject(fieldIndex);

						if (!this.FollowNode(fieldIndex, ref entity, state))
							return false;
					}

					return true;

				default:
					state.ReadingAction = LegacyReaderState.ReadingActionType.ReadHeader;

					if (this.array != null)
						return this.array(ref entity, state);

					if (!this.HoldValue)
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
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadSingle()));

							return true;

						case ProtoBuf.WireType.Fixed64:
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadDouble()));

							return true;

						case ProtoBuf.WireType.String:
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadString()));

							return true;

						case ProtoBuf.WireType.Variant:
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadInt64()));

							return true;
					}

					state.Error("wire type not supported, skipped");
					state.Reader.SkipField();

					return true;
			}
		}

		#endregion

		#region Methods / Private

		private static bool Ignore(LegacyReaderState state)
		{
			var dummy = default(TEntity);

			return LegacyReader<TEntity>.emptyReader.Read(ref dummy, state);
		}

		private bool FollowNode(int fieldIndex, ref TEntity target, LegacyReaderState state)
		{
			state.ReadingAction = LegacyReaderState.ReadingActionType.ReadValue;

			return fieldIndex < this.objectFields.Count && this.objectFields[fieldIndex] != null
				? this.objectFields[fieldIndex](ref target, state)
				: LegacyReader<TEntity>.Ignore(state);
		}

		private bool ReadObjectValue(ref TEntity target, LegacyReaderState state)
		{
			int visitCount;
			SubItemToken lastSubItem;

			lastSubItem = ProtoReader.StartSubItem(state.Reader);

			if (!state.EnterObject(out visitCount))
				return false;

			while (ProtoReader.HasSubValue(ProtoBuf.WireType.None, state.Reader))
			{
				int fieldIndex;

				state.ReadHeader(out fieldIndex);
				state.AddObject(fieldIndex);

				if (visitCount == 1 && fieldIndex < this.objectFields.Count && this.objectFields[fieldIndex] != null)
				{
					state.ReadingAction = LegacyReaderState.ReadingActionType.ReadValue;

					if (!this.objectFields[fieldIndex](ref target, state))
						return false;
				}
				else
				{
					int index = visitCount - 1;

					state.ReadingAction = LegacyReaderState.ReadingActionType.UseHeader;

					if (!(index < this.arrayFields.Count && this.arrayFields[index] != null
						? this.arrayFields[index](ref target, state)
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
			SubItemToken lastSubItem;

			if (this.HoldValue)
				return this.ReadValueArray(constructor, state);

			state.ReadingAction = LegacyReaderState.ReadingActionType.ReadHeader;

			lastSubItem = ProtoReader.StartSubItem(state.Reader);

			if (!state.EnterObject(out dummy))
			{
				return (int index, out TEntity current) =>
				{
					current = default(TEntity);
					return BrowserState.Failure;
				};
			}

			return (int index, out TEntity current) =>
			{
				state.AddObject(index);

				if (!ProtoReader.HasSubValue(ProtoBuf.WireType.None, state.Reader))
				{
					current = default(TEntity);

					state.LeaveObject();

					ProtoReader.EndSubItem(lastSubItem, state.Reader);

					return BrowserState.Success;
				}

				current = constructor();

				if (this.Read(ref current, state))
					return BrowserState.Continue;

				current = default(TEntity);

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
					current = default(TEntity);

					return BrowserState.Success;
				}

				current = constructor();

				if (!this.Read(ref current, state))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}

		#endregion
	}
}
